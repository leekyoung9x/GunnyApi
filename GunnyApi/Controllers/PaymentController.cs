using GunnyApi.Infrastructure.Controllers;
using GunnyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GunnyApi.Infrastructure.Context;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Infrastructure.Services;

namespace GunnyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : BaseApiController
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PaymentTiersSettings _paymentTiersSettings;
    private readonly ILocalizationService _localization;

    public PaymentController(
        ILogger<PaymentController> logger,
        IConfiguration configuration,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        IOptions<PaymentTiersSettings> paymentTiersSettings,
        ILocalizationService localization)
    {
        _logger = logger;
        _configuration = configuration;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _paymentTiersSettings = paymentTiersSettings.Value;
        _localization = localization;
    }

    /// <summary>
    /// Lấy danh sách các mốc nạp tiền và phần thưởng
    /// </summary>
    [HttpGet("tiers")]
    [AllowAnonymous]
    public IActionResult GetPaymentTiers()
    {
        try
        {
            if (!_paymentTiersSettings.Enabled)
            {
                return Ok(new
                {
                    success = false,
                    message = "Payment tiers feature is disabled",
                    data = new List<PaymentTier>()
                });
            }

            // Lọc và sắp xếp các tiers đang active
            var activeTiers = _paymentTiersSettings.Tiers
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Amount)
                .Select(t => new
                {
                    t.Id,
                    t.Amount,
                    t.Gold,
                    t.Money,
                    t.GiftToken,
                    t.BonusPercent,
                    DisplayName = !string.IsNullOrEmpty(t.DisplayNameKey) 
                        ? _localization.GetString(t.DisplayNameKey) 
                        : t.DisplayName,
                    Description = !string.IsNullOrEmpty(t.DescriptionKey) 
                        ? _localization.GetString(t.DescriptionKey) 
                        : t.Description,
                    t.IsActive,
                    t.SortOrder
                })
                .ToList();

            return Ok(new
            {
                success = true,
                message = "Payment tiers retrieved successfully",
                currency = _paymentTiersSettings.Currency,
                minimumAmount = _paymentTiersSettings.MinimumAmount,
                maximumAmount = _paymentTiersSettings.MaximumAmount,
                data = activeTiers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment tiers");
            return StatusCode(500, new
            {
                success = false,
                message = _localization.GetString("Error.Generic"),
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết một mốc nạp tiền theo ID
    /// </summary>
    [HttpGet("tiers/{id}")]
    [AllowAnonymous]
    public IActionResult GetPaymentTierById(int id)
    {
        try
        {
            var tier = _paymentTiersSettings.Tiers
                .FirstOrDefault(t => t.Id == id && t.IsActive);

            if (tier == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Payment tier not found"
                });
            }

            // Translate tier data
            var localizedTier = new
            {
                tier.Id,
                tier.Amount,
                tier.Gold,
                tier.Money,
                tier.GiftToken,
                tier.BonusPercent,
                DisplayName = !string.IsNullOrEmpty(tier.DisplayNameKey) 
                    ? _localization.GetString(tier.DisplayNameKey) 
                    : tier.DisplayName,
                Description = !string.IsNullOrEmpty(tier.DescriptionKey) 
                    ? _localization.GetString(tier.DescriptionKey) 
                    : tier.Description,
                tier.IsActive,
                tier.SortOrder
            };

            return Ok(new
            {
                success = true,
                message = "Payment tier retrieved successfully",
                data = localizedTier
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment tier {TierId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = _localization.GetString("Error.Generic"),
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Tạo Checkout Session với PayMongo
    /// </summary>
    [HttpPost("create-checkout-session")]
    [Authorize]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
    {
        try
        {
            // Lấy thông tin user từ token
            var username = _userContext.Username;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = _localization.GetString("Payment.Unauthorized") });
            }

            _logger.LogInformation("User {Username} đang tạo checkout session với amount: {Amount}", 
                username, request.Amount);

            // Lấy cấu hình từ appsettings
            var apiUrl = _configuration["PayMongoSettings:ApiUrl"];
            var secretKey = _configuration["PayMongoSettings:SecretKey"];
            var currency = _configuration["PayMongoSettings:Currency"] ?? "PHP";
            var sendEmailReceipt = bool.Parse(_configuration["PayMongoSettings:SendEmailReceipt"] ?? "false");
            var showDescription = bool.Parse(_configuration["PayMongoSettings:ShowDescription"] ?? "true");
            var showLineItems = bool.Parse(_configuration["PayMongoSettings:ShowLineItems"] ?? "true");
            var paymentMethods = _configuration.GetSection("PayMongoSettings:PaymentMethods").Get<List<string>>() 
                ?? new List<string> { "qrph", "card", "gcash" };

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("PayMongo settings chưa được cấu hình");
                return StatusCode(500, new { message = _localization.GetString("Payment.ConfigNotSetup") });
            }

            // Tạo request body cho PayMongo
            var payMongoRequest = new PayMongoCheckoutSessionRequest
            {
                Data = new PayMongoCheckoutData
                {
                    Attributes = new PayMongoCheckoutAttributes
                    {
                        SendEmailReceipt = sendEmailReceipt,
                        ShowDescription = showDescription,
                        ShowLineItems = showLineItems,
                        PaymentMethodTypes = paymentMethods,
                        Description = !string.IsNullOrEmpty(request.Description) 
                            ? request.Description 
                            : _localization.GetString("Payment.PaymentFor", username),
                        LineItems = new List<PayMongoLineItem>
                        {
                            new PayMongoLineItem
                            {
                                Currency = currency,
                                Amount = request.Amount, // Amount từ client (đơn vị: centavos)
                                Description = !string.IsNullOrEmpty(request.Description) 
                                    ? request.Description 
                                    : _localization.GetString("Payment.TopupFor", username),
                                Name = !string.IsNullOrEmpty(request.ProductName) 
                                    ? request.ProductName 
                                    : "Nạp tiền",
                                Quantity = request.Quantity > 0 ? request.Quantity : 1,
                                Images = !string.IsNullOrEmpty(request.ImageUrl) 
                                    ? new List<string> { request.ImageUrl } 
                                    : null
                            }
                        },
                        Metadata = new Dictionary<string, object>
                        {
                            { "username", username },
                            { "user_id", _userContext.UserId ?? 0 }
                        }
                    }
                }
            };

            // Gọi PayMongo API
            var httpClient = _httpClientFactory.CreateClient();
            var endpoint = $"{apiUrl}/checkout_sessions";

            // Tạo Basic Auth từ secret key (encode base64)
            var authBytes = Encoding.UTF8.GetBytes($"{secretKey}:");
            var authHeader = Convert.ToBase64String(authBytes);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

            var jsonContent = JsonSerializer.Serialize(payMongoRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = false
            });

            _logger.LogInformation("Gửi request tới PayMongo: {Endpoint}", endpoint);
            _logger.LogDebug("Request body: {Body}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayMongo API trả về lỗi: {StatusCode} - {Response}", 
                    response.StatusCode, responseContent);
                return StatusCode((int)response.StatusCode, new 
                { 
                    message = _localization.GetString("Payment.CannotCreateSession"), 
                    error = responseContent 
                });
            }

            // Parse response từ PayMongo
            var payMongoResponse = JsonSerializer.Deserialize<PayMongoCheckoutSessionResponse>(
                responseContent, 
                new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    PropertyNameCaseInsensitive = true 
                });

            if (payMongoResponse?.Data == null)
            {
                _logger.LogError("PayMongo response không hợp lệ");
                return StatusCode(500, new { message = _localization.GetString("Payment.InvalidResponse") });
            }

            // Log thành công
            _logger.LogInformation(
                "Tạo checkout session thành công - User: {Username}, CheckoutId: {CheckoutId}, Amount: {Amount}",
                username, payMongoResponse.Data.Id, request.Amount);

            // Trả về response cho client
            var result = new CreateCheckoutSessionResponse
            {
                CheckoutSessionId = payMongoResponse.Data.Id,
                CheckoutUrl = payMongoResponse.Data.Attributes.CheckoutUrl,
                ClientKey = payMongoResponse.Data.Attributes.ClientKey,
                Description = payMongoResponse.Data.Attributes.Description,
                Amount = request.Amount,
                Status = payMongoResponse.Data.Attributes.Status
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo checkout session");
            return StatusCode(500, new { message = _localization.GetString("Payment.CreateCheckoutError", ex.Message) });
        }
    }

    /// <summary>
    /// PayMongo Webhook Endpoint
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayMongoWebhook([FromBody] PayMongoWebhookRequest webhookData)
    {
        try
        {
            // Log thông tin webhook nhận được
            _logger.LogInformation("PayMongo Webhook received: {WebhookData}", JsonSerializer.Serialize(webhookData));

            if (webhookData?.Data == null)
            {
                _logger.LogWarning("Webhook data is null or invalid");
                return BadRequest(new { message = "Invalid webhook data" });
            }

            // Xử lý theo loại event
            switch (webhookData.Data.Attributes?.Type)
            {
                case "checkout_session.payment.paid":
                    await HandlePaymentPaid(webhookData.Data);
                    break;

                case "checkout_session.payment.failed":
                    await HandlePaymentFailed(webhookData.Data);
                    break;

                case "checkout_session.payment.refunded":
                    await HandlePaymentRefunded(webhookData.Data);
                    break;

                case "checkout_session.source.chargeable":
                    await HandleSourceChargeable(webhookData.Data);
                    break;

                default:
                    _logger.LogInformation("Unhandled webhook type: {Type}", webhookData.Data.Attributes?.Type);
                    break;
            }

            // PayMongo yêu cầu trả về 200 OK
            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayMongo webhook");
            // Vẫn trả về 200 OK để tránh PayMongo retry liên tục
            return Ok(new { message = "Webhook received", error = ex.Message });
        }
    }

    /// <summary>
    /// Xử lý khi payment thành công
    /// </summary>
    private async Task HandlePaymentPaid(PayMongoEventData data)
    {
        _logger.LogInformation("Processing payment paid event: {EventId}", data.Id);
        
        var eventAttributes = data.Attributes;
        if (eventAttributes?.Data == null)
        {
            _logger.LogWarning("Event data is null");
            return;
        }

        var checkoutSession = eventAttributes.Data.Attributes;
        if (checkoutSession == null)
        {
            _logger.LogWarning("Checkout session attributes is null");
            return;
        }

        // Lấy thông tin payment từ checkout session
        var payment = checkoutSession.Payments?.FirstOrDefault();
        if (payment?.Attributes == null)
        {
            _logger.LogWarning("No payment found in checkout session");
            return;
        }

        var paymentAttrs = payment.Attributes;
        
        // Log thông tin chi tiết
        _logger.LogInformation(
            "Payment successful - Checkout ID: {CheckoutId}, Payment ID: {PaymentId}, Amount: {Amount} {Currency}, Net: {NetAmount}, Fee: {Fee}, Status: {Status}, Method: {Method}",
            eventAttributes.Data.Id,
            payment.Id,
            paymentAttrs.Amount,
            paymentAttrs.Currency,
            paymentAttrs.NetAmount,
            paymentAttrs.Fee,
            paymentAttrs.Status,
            checkoutSession.PaymentMethodUsed
        );

        // Log billing info
        if (paymentAttrs.Billing != null)
        {
            _logger.LogInformation(
                "Billing info - Name: {Name}, Email: {Email}, Phone: {Phone}",
                paymentAttrs.Billing.Name,
                paymentAttrs.Billing.Email,
                paymentAttrs.Billing.Phone
            );
        }

        // Log line items
        if (checkoutSession.LineItems != null)
        {
            foreach (var item in checkoutSession.LineItems)
            {
                _logger.LogInformation(
                    "Line item - {Name}: {Quantity} x {Amount} {Currency}",
                    item.Name,
                    item.Quantity,
                    item.Amount,
                    item.Currency
                );
            }
        }

        // Cộng tiền vào Mem_Account
        try
        {
            // Lấy amount từ payment (đơn vị: centavos)
            var amountInCentavos = paymentAttrs.Amount;

            // Lấy username từ metadata
            string? username = null;
            if (checkoutSession.Metadata != null && checkoutSession.Metadata.ContainsKey("username"))
            {
                username = checkoutSession.Metadata["username"]?.ToString();
            }

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Không tìm thấy username trong metadata để cộng tiền");
                return;
            }

            var memberConnectionString = _configuration.GetConnectionString("DefaultConnection");
            using var memberConnection = new SqlConnection(memberConnectionString);
            await memberConnection.OpenAsync();

            // Tìm UserID từ Email (Email trong Mem_Account chính là username)
            var getUserIdSql = "SELECT UserID FROM Mem_Account WHERE Email = @Username";
            var userId = await memberConnection.ExecuteScalarAsync<int?>(
                getUserIdSql,
                new { Username = username }
            );

            if (!userId.HasValue)
            {
                _logger.LogWarning("Không tìm thấy user với username: {Username}", username);
                return;
            }

            // Cộng tiền vào Mem_Account (amount đã tính bằng centavos)
            var updateMoneySql = "UPDATE Mem_Account SET Money = Money + @Amount WHERE UserID = @UserId";
            var rowsAffected = await memberConnection.ExecuteAsync(
                updateMoneySql,
                new { UserId = userId.Value, Amount = amountInCentavos }
            );

            if (rowsAffected > 0)
            {
                // Lấy số dư mới
                var newBalance = await memberConnection.ExecuteScalarAsync<long>(
                    "SELECT Money FROM Mem_Account WHERE UserID = @UserId",
                    new { UserId = userId.Value }
                );

                _logger.LogInformation(
                    "Đã cộng {Amount} vào tài khoản UserID: {UserId}, Username: {Username}. Số dư mới: {NewBalance}",
                    amountInCentavos,
                    userId.Value,
                    username,
                    newBalance
                );
            }
            else
            {
                _logger.LogWarning("Không thể cộng tiền vào tài khoản UserID: {UserId}", userId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cộng tiền vào Mem_Account");
        }
    }

    /// <summary>
    /// Xử lý khi payment thất bại
    /// </summary>
    private async Task HandlePaymentFailed(PayMongoEventData data)
    {
        _logger.LogWarning("Processing payment failed event: {EventId}", data.Id);
        
        var checkoutSession = data.Attributes?.Data?.Attributes;
        if (checkoutSession != null)
        {
            _logger.LogWarning(
                "Payment failed - Checkout ID: {CheckoutId}, Customer: {CustomerId}, Email: {Email}",
                data.Attributes?.Data?.Id,
                checkoutSession.CustomerId,
                checkoutSession.CustomerEmail
            );
        }
        
        // TODO: Implement your business logic here
        // - Cập nhật trạng thái đơn hàng thành failed
        // - Gửi thông báo cho user qua email
        // - Log lý do thất bại nếu có
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Xử lý khi payment bị refund
    /// </summary>
    private async Task HandlePaymentRefunded(PayMongoEventData data)
    {
        _logger.LogInformation("Processing payment refunded event: {EventId}", data.Id);
        
        var checkoutSession = data.Attributes?.Data?.Attributes;
        var payment = checkoutSession?.Payments?.FirstOrDefault();
        
        if (payment?.Attributes != null)
        {
            _logger.LogInformation(
                "Payment refunded - Payment ID: {PaymentId}, Amount: {Amount} {Currency}",
                payment.Id,
                payment.Attributes.Amount,
                payment.Attributes.Currency
            );
        }
        
        // TODO: Implement your business logic here
        // - Trừ tiền trong tài khoản user (số tiền: payment.Attributes.Amount)
        // - Cập nhật trạng thái đơn hàng thành refunded
        // - Gửi email thông báo refund
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Xử lý khi source chargeable (cho GCash, GrabPay)
    /// </summary>
    private async Task HandleSourceChargeable(PayMongoEventData data)
    {
        _logger.LogInformation("Processing source chargeable event: {EventId}", data.Id);
        
        var checkoutSession = data.Attributes?.Data?.Attributes;
        if (checkoutSession != null)
        {
            _logger.LogInformation(
                "Source chargeable - Checkout ID: {CheckoutId}, Method: {Method}",
                data.Attributes?.Data?.Id,
                checkoutSession.PaymentMethodUsed
            );
        }
        
        // TODO: Implement your business logic here
        // - Tạo payment từ source (cho e-wallet như GCash, GrabPay)
        // - Cập nhật trạng thái đơn hàng
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Debug Webhook Endpoint - Nhận raw JSON để xem PayMongo gửi gì
    /// </summary>
    [HttpPost("webhook/debug")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugWebhook([FromBody] dynamic webhookData)
    {
        try
        {
            // Serialize ra JSON với format đẹp
            var jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string jsonString = JsonSerializer.Serialize(webhookData, jsonOptions);
            
            // Log ra console
            Console.WriteLine("=== PAYMONGO WEBHOOK DEBUG ===");
            Console.WriteLine(jsonString);
            Console.WriteLine("=== END WEBHOOK DEBUG ===");
            
            // Log vào logger
            _logger.LogInformation("PayMongo Webhook Debug - Raw JSON: {Json}", jsonString);
            
            // Lấy thêm headers để debug
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            var headersJson = JsonSerializer.Serialize(headers, jsonOptions);
            Console.WriteLine("=== WEBHOOK HEADERS ===");
            Console.WriteLine(headersJson);
            Console.WriteLine("=== END HEADERS ===");
            
            return Ok(new 
            { 
                message = "Debug webhook received - check console output",
                receivedAt = DateTime.UtcNow,
                dataReceived = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in debug webhook: {ex.Message}");
            _logger.LogError(ex, "Error processing debug webhook");
            return Ok(new { message = "Error received", error = ex.Message });
        }
    }

    /// <summary>
    /// Alternative Debug Endpoint - Nhận object và log raw body
    /// </summary>
    [HttpPost("webhook/raw")]
    [AllowAnonymous]
    public async Task<IActionResult> RawWebhook()
    {
        try
        {
            // Đọc raw body
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();
            
            // Log ra console
            Console.WriteLine("=== PAYMONGO RAW BODY ===");
            Console.WriteLine(rawBody);
            Console.WriteLine("=== END RAW BODY ===");
            
            _logger.LogInformation("PayMongo Raw Body: {Body}", rawBody);
            
            // Parse và format lại
            try
            {
                var jsonDoc = JsonDocument.Parse(rawBody);
                var formatted = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("=== FORMATTED JSON ===");
                Console.WriteLine(formatted);
                Console.WriteLine("=== END FORMATTED ===");
            }
            catch
            {
                Console.WriteLine("Could not parse as JSON");
            }
            
            return Ok(new { message = "Raw webhook received", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Ok(new { message = "Error", error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint để kiểm tra controller hoạt động
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult GetStatus()
    {
        return Ok(new 
        { 
            status = "active",
            service = "Payment Controller",
            timestamp = DateTime.UtcNow,
            endpoints = new[]
            {
                "/api/payment/webhook - Main webhook endpoint",
                "/api/payment/webhook/debug - Debug với dynamic object",
                "/api/payment/webhook/raw - Debug với raw body",
                "/api/payment/status - Status check"
            }
        });
    }
}
