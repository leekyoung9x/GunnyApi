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
using GunnyApi.Repositories;

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
    private readonly IPaymentRepository _paymentRepository;

    public PaymentController(
        ILogger<PaymentController> logger,
        IConfiguration configuration,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        IOptions<PaymentTiersSettings> paymentTiersSettings,
        ILocalizationService localization,
        IPaymentRepository paymentRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _paymentTiersSettings = paymentTiersSettings.Value;
        _localization = localization;
        _paymentRepository = paymentRepository;
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
                    amount = t.Amount, // Số tiền tính bằng PHP (client gửi giá trị này vào API CreateCheckoutSession)
                    rewards = new
                    {
                        money = t.Money, // Xu
                        gold = _paymentTiersSettings.EnableGoldReward ? t.Gold : 0, // Vàng (nếu enabled trong config)
                        giftToken = _paymentTiersSettings.EnableGiftTokenReward ? t.GiftToken : 0 // Lễ Kim (nếu enabled trong config)
                    },
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
    /// Lấy lịch sử giao dịch của user hiện tại
    /// </summary>
    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (!_userContext.UserId.HasValue)
            {
                return Unauthorized(new { message = _localization.GetString("Payment.Unauthorized") });
            }

            // Validate pagination
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var userId = _userContext.UserId.Value;
            
            // Get payment histories
            var histories = await _paymentRepository.GetPaymentHistoriesByUserIdAsync(userId, pageNumber, pageSize);
            var totalCount = await _paymentRepository.GetPaymentHistoryCountByUserIdAsync(userId);

            // Map to DTO
            var historyDtos = histories.Select(h => new PaymentHistoryDto
            {
                Id = h.Id,
                Amount = h.Amount,
                Currency = h.Currency,
                Status = h.Status,
                PaymentMethod = h.PaymentMethod,
                Description = h.Description,
                ProductName = h.ProductName,
                MoneyReward = h.MoneyReward,
                GoldReward = h.GoldReward,
                GiftTokenReward = h.GiftTokenReward,
                CheckoutSessionId = h.CheckoutSessionId,
                PaymentId = h.PaymentId,
                CheckoutUrl = h.CheckoutUrl,
                CreatedAt = h.CreatedAt,
                PaidAt = h.PaidAt,
                ExpiresAt = h.ExpiresAt
            }).ToList();

            var response = new PaymentHistoryResponse
            {
                Success = true,
                Message = _localization.GetString("Payment.HistoryRetrievedSuccess"),
                Data = historyDtos,
                TotalCount = totalCount
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history for user {UserId}", _userContext.UserId);
            return StatusCode(500, new
            {
                success = false,
                message = _localization.GetString("Payment.HistoryRetrievedError", ex.Message)
            });
        }
    }

    /// <summary>
    /// Lấy chi tiết một giao dịch cụ thể
    /// </summary>
    [HttpGet("history/{id}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistoryById(int id)
    {
        try
        {
            if (!_userContext.UserId.HasValue)
            {
                return Unauthorized(new { message = _localization.GetString("Payment.Unauthorized") });
            }

            var paymentHistory = await _paymentRepository.GetPaymentHistoryByIdAsync(id);

            if (paymentHistory == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = _localization.GetString("Payment.HistoryNotFound")
                });
            }

            // Verify user owns this payment history
            if (paymentHistory.UserId != _userContext.UserId.Value)
            {
                return Forbid();
            }

            var historyDto = new PaymentHistoryDto
            {
                Id = paymentHistory.Id,
                Amount = paymentHistory.Amount,
                Currency = paymentHistory.Currency,
                Status = paymentHistory.Status,
                PaymentMethod = paymentHistory.PaymentMethod,
                Description = paymentHistory.Description,
                ProductName = paymentHistory.ProductName,
                MoneyReward = paymentHistory.MoneyReward,
                GoldReward = paymentHistory.GoldReward,
                GiftTokenReward = paymentHistory.GiftTokenReward,
                CheckoutSessionId = paymentHistory.CheckoutSessionId,
                PaymentId = paymentHistory.PaymentId,
                CheckoutUrl = paymentHistory.CheckoutUrl,
                CreatedAt = paymentHistory.CreatedAt,
                PaidAt = paymentHistory.PaidAt,
                ExpiresAt = paymentHistory.ExpiresAt
            };

            return Ok(new
            {
                success = true,
                message = "Payment history retrieved successfully",
                data = historyDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = _localization.GetString("Error.Generic"),
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Hủy (Expire) một Checkout Session - Sử dụng khi user muốn hủy giao dịch đang chờ
    /// </summary>
    [HttpPost("expire-checkout-session")]
    [Authorize]
    public async Task<IActionResult> ExpireCheckoutSession([FromBody] ExpireCheckoutSessionRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.CheckoutSessionId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localization.GetString("Payment.ExpireInvalidSession")
                });
            }

            // Lấy thông tin user từ token
            var username = _userContext.Username;
            if (string.IsNullOrEmpty(username) || !_userContext.UserId.HasValue)
            {
                return Unauthorized(new { message = _localization.GetString("Payment.Unauthorized") });
            }

            // Kiểm tra payment history có tồn tại và thuộc về user không
            var paymentHistory = await _paymentRepository.GetPaymentHistoryByCheckoutSessionIdAsync(request.CheckoutSessionId);
            
            if (paymentHistory == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = _localization.GetString("Payment.ExpireNotFound")
                });
            }

            // Verify user owns this payment
            if (paymentHistory.UserId != _userContext.UserId.Value)
            {
                return Forbid();
            }

            // Kiểm tra trạng thái - chỉ expire được nếu đang pending
            if (paymentHistory.Status != "pending")
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localization.GetString("Payment.ExpireAlreadyExpired"),
                    currentStatus = paymentHistory.Status
                });
            }

            _logger.LogInformation(
                "User {Username} (ID: {UserId}) đang expire checkout session: {CheckoutSessionId}",
                username, _userContext.UserId.Value, request.CheckoutSessionId);

            // Lấy cấu hình PayMongo
            var apiUrl = _configuration["PayMongoSettings:ApiUrl"];
            var secretKey = _configuration["PayMongoSettings:SecretKey"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("PayMongo settings chưa được cấu hình");
                return StatusCode(500, new { message = _localization.GetString("Payment.ConfigNotSetup") });
            }

            // Gọi PayMongo API để expire checkout session
            var httpClient = _httpClientFactory.CreateClient();
            var endpoint = $"{apiUrl}/checkout_sessions/{request.CheckoutSessionId}/expire";

            // Tạo Basic Auth
            var authBytes = Encoding.UTF8.GetBytes($"{secretKey}:");
            var authHeader = Convert.ToBase64String(authBytes);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

            _logger.LogInformation("Gửi expire request tới PayMongo: {Endpoint}", endpoint);

            // PayMongo expire endpoint là POST request với empty body
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayMongo API trả về lỗi khi expire: {StatusCode} - {Response}", 
                    response.StatusCode, responseContent);
                
                // Parse error message nếu có
                string errorMessage = responseContent;
                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (errorJson.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
                    {
                        var firstError = errors[0];
                        if (firstError.TryGetProperty("detail", out var detail))
                        {
                            errorMessage = detail.GetString() ?? responseContent;
                        }
                    }
                }
                catch
                {
                    // Ignore parse error
                }

                return StatusCode((int)response.StatusCode, new 
                { 
                    success = false,
                    message = _localization.GetString("Payment.ExpireError", errorMessage)
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
                _logger.LogError("PayMongo expire response không hợp lệ");
                return StatusCode(500, new { message = _localization.GetString("Payment.InvalidResponse") });
            }

            // Cập nhật status trong database
            var expiredAt = DateTime.Now;
            var updateSuccess = await _paymentRepository.UpdatePaymentStatusAsync(
                request.CheckoutSessionId, 
                "expired", 
                expiredAt
            );

            if (!updateSuccess)
            {
                _logger.LogWarning("Không thể cập nhật payment history sau khi expire thành công trên PayMongo");
            }

            _logger.LogInformation(
                "Expire checkout session thành công - User: {Username}, CheckoutId: {CheckoutId}, Status: {Status}",
                username, request.CheckoutSessionId, payMongoResponse.Data.Attributes.Status);

            // Trả về response
            var result = new ExpireCheckoutSessionResponse
            {
                Success = true,
                Message = _localization.GetString("Payment.ExpireSuccess"),
                Data = new ExpireCheckoutSessionData
                {
                    CheckoutSessionId = payMongoResponse.Data.Id ?? string.Empty,
                    Status = payMongoResponse.Data.Attributes.Status ?? "expired",
                    ExpiredAt = expiredAt
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi expire checkout session: {CheckoutSessionId}", request.CheckoutSessionId);
            return StatusCode(500, new 
            { 
                success = false,
                message = _localization.GetString("Payment.ExpireError", ex.Message) 
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

            // Validate amount phải khớp với một tier
            // Client gửi amount bằng PHP (VD: 50 PHP)
            // Server sẽ convert sang centavos khi tạo giao dịch PayMongo (50 PHP * 100 = 5000 centavos)
            var matchingTier = _paymentTiersSettings.Tiers
                .FirstOrDefault(t => t.IsActive && t.Amount == request.Amount);

            if (matchingTier == null)
            {
                _logger.LogWarning(
                    "Client gửi amount {Amount} PHP không khớp với tier nào. Available tiers: {Tiers}",
                    request.Amount,
                    string.Join(", ", _paymentTiersSettings.Tiers.Where(t => t.IsActive).Select(t => $"{t.Amount} PHP"))
                );
                return BadRequest(new 
                { 
                    message = "Invalid payment amount. Amount must match one of the available tiers.",
                    requestedAmount = request.Amount,
                    availableTiers = _paymentTiersSettings.Tiers
                        .Where(t => t.IsActive)
                        .Select(t => new { amount = t.Amount, currency = "PHP" })
                        .ToList()
                });
            }

            // Convert PHP sang centavos cho PayMongo
            var amountInCentavos = (int)(request.Amount * 100);

            _logger.LogInformation(
                "User {Username} đang tạo checkout session - Amount: {Amount} PHP ({Centavos} centavos) - Tier: {TierId}", 
                username, request.Amount, amountInCentavos, matchingTier.Id);

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
                                Amount = amountInCentavos, // Amount đã convert sang centavos (VD: 50 PHP * 100 = 5000 centavos)
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

            // Lưu vào Payment_History
            try
            {
                // PayMongo không trả về ExpiresAt trong response, sẽ set null
                DateTime? expiresAt = null;

                var paymentHistory = new PaymentHistory
                {
                    UserId = _userContext.UserId ?? 0,
                    Username = username,
                    Amount = request.Amount,
                    AmountInCentavos = amountInCentavos,
                    Currency = currency,
                    CheckoutSessionId = payMongoResponse.Data.Id,
                    CheckoutUrl = payMongoResponse.Data.Attributes.CheckoutUrl,
                    TierId = matchingTier.Id,
                    MoneyReward = matchingTier.Money,
                    GoldReward = matchingTier.Gold,
                    GiftTokenReward = matchingTier.GiftToken,
                    Status = "pending",
                    Description = request.Description,
                    ProductName = request.ProductName,
                    Metadata = JsonSerializer.Serialize(payMongoResponse.Data.Attributes.Metadata),
                    CreatedAt = DateTime.Now,
                    ExpiresAt = expiresAt
                };

                var paymentHistoryId = await _paymentRepository.CreatePaymentHistoryAsync(paymentHistory);
                
                _logger.LogInformation(
                    "Đã lưu payment history với ID: {PaymentHistoryId} cho checkout session: {CheckoutSessionId}",
                    paymentHistoryId, payMongoResponse.Data.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu payment history, nhưng vẫn trả về checkout URL cho user");
                // Không throw exception, vẫn cho phép user thanh toán
            }

            // Trả về response cho client
            var result = new CreateCheckoutSessionResponse
            {
                CheckoutSessionId = payMongoResponse.Data.Id ?? string.Empty,
                CheckoutUrl = payMongoResponse.Data.Attributes.CheckoutUrl ?? string.Empty,
                ClientKey = payMongoResponse.Data.Attributes.ClientKey ?? string.Empty,
                Description = payMongoResponse.Data.Attributes.Description ?? string.Empty,
                Amount = amountInCentavos, // Trả về centavos để client biết số tiền thực tế trên PayMongo
                Status = payMongoResponse.Data.Attributes.Status ?? string.Empty
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
            // Lấy amount từ payment (đơn vị: centavos cho PHP, tùy theo currency)
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

            // Tìm payment tier tương ứng với số tiền đã thanh toán
            var matchedTier = _paymentTiersSettings.Tiers
                .FirstOrDefault(t => t.IsActive && t.Amount * 100 == amountInCentavos);

            if (matchedTier == null)
            {
                _logger.LogWarning(
                    "Không tìm thấy payment tier phù hợp với amount: {Amount} centavos (expected: amount * 100). Payment bị từ chối.",
                    amountInCentavos
                );
                return;
            }

            _logger.LogInformation(
                "Matched payment tier: ID={TierId}, Amount={Amount}, Money={Money}, Gold={Gold}, GiftToken={GiftToken}",
                matchedTier.Id,
                matchedTier.Amount,
                matchedTier.Money,
                matchedTier.Gold,
                matchedTier.GiftToken
            );

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

            // Cộng Money từ config tier vào Mem_Account
            var updateMoneySql = "UPDATE Mem_Account SET Money = Money + @Money WHERE UserID = @UserId";
            var rowsAffected = await memberConnection.ExecuteAsync(
                updateMoneySql,
                new { UserId = userId.Value, Money = matchedTier.Money }
            );

            if (rowsAffected > 0)
            {
                // Lấy số dư mới
                var newBalance = await memberConnection.ExecuteScalarAsync<long>(
                    "SELECT Money FROM Mem_Account WHERE UserID = @UserId",
                    new { UserId = userId.Value }
                );

                _logger.LogInformation(
                    "Đã cộng {Money} Xu (từ tier {TierId}) vào tài khoản UserID: {UserId}, Username: {Username}. Số dư mới: {NewBalance}. Payment amount: {PaymentAmount} centavos",
                    matchedTier.Money,
                    matchedTier.Id,
                    userId.Value,
                    username,
                    newBalance,
                    amountInCentavos
                );

                // Cộng Gold và GiftToken vào Tank database nếu được bật trong config
                bool shouldAddGold = _paymentTiersSettings.EnableGoldReward && matchedTier.Gold > 0;
                bool shouldAddGiftToken = _paymentTiersSettings.EnableGiftTokenReward && matchedTier.GiftToken > 0;

                if (shouldAddGold || shouldAddGiftToken)
                {
                    try
                    {
                        var tankConnectionString = _configuration.GetConnectionString("TankConnection");
                        using var tankConnection = new SqlConnection(tankConnectionString);
                        await tankConnection.OpenAsync();

                        // Xây dựng câu SQL động dựa trên config
                        var updateFields = new List<string>();
                        var parameters = new DynamicParameters();
                        parameters.Add("Username", username);

                        if (shouldAddGold)
                        {
                            updateFields.Add("Gold = Gold + @Gold");
                            parameters.Add("Gold", matchedTier.Gold);
                        }

                        if (shouldAddGiftToken)
                        {
                            updateFields.Add("GiftToken = GiftToken + @GiftToken");
                            parameters.Add("GiftToken", matchedTier.GiftToken);
                        }

                        var updateTankSql = $@"
                            UPDATE dbo.Users 
                            SET {string.Join(", ", updateFields)}
                            WHERE UserName = @Username";

                        var tankRowsAffected = await tankConnection.ExecuteAsync(updateTankSql, parameters);

                        if (tankRowsAffected > 0)
                        {
                            var rewardParts = new List<string>();
                            if (shouldAddGold) rewardParts.Add($"{matchedTier.Gold} Gold");
                            if (shouldAddGiftToken) rewardParts.Add($"{matchedTier.GiftToken} Lễ Kim");

                            _logger.LogInformation(
                                "Đã cộng {Rewards} vào Tank database cho user: {Username}",
                                string.Join(" và ", rewardParts),
                                username
                            );
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Không tìm thấy user trong Tank database hoặc không thể cộng Gold/GiftToken: {Username}",
                                username
                            );
                        }
                    }
                    catch (Exception tankEx)
                    {
                        _logger.LogError(tankEx, "Lỗi khi cộng Gold/GiftToken vào Tank database cho user: {Username}", username);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Không thể cộng tiền vào tài khoản UserID: {UserId}", userId.Value);
            }

            // Cập nhật Payment_History
            try
            {
                var paymentHistory = await _paymentRepository.GetPaymentHistoryByCheckoutSessionIdAsync(eventAttributes.Data.Id ?? "");
                
                if (paymentHistory != null)
                {
                    paymentHistory.PaymentIntentId = payment.Attributes.PaymentIntentId ?? string.Empty;
                    paymentHistory.PaymentId = payment.Id ?? string.Empty;
                    paymentHistory.Status = "paid";
                    paymentHistory.PaymentMethod = checkoutSession.PaymentMethodUsed;
                    paymentHistory.EventType = eventAttributes.Type;
                    paymentHistory.EventId = data.Id;
                    paymentHistory.UpdatedAt = DateTime.Now;
                    paymentHistory.PaidAt = DateTime.Now;
                    paymentHistory.Metadata = JsonSerializer.Serialize(new
                    {
                        payment = new
                        {
                            id = payment.Id,
                            amount = paymentAttrs.Amount,
                            netAmount = paymentAttrs.NetAmount,
                            fee = paymentAttrs.Fee,
                            billing = paymentAttrs.Billing
                        },
                        checkoutSession = new
                        {
                            id = eventAttributes.Data.Id,
                            paymentMethod = checkoutSession.PaymentMethodUsed,
                            lineItems = checkoutSession.LineItems
                        }
                    });

                    await _paymentRepository.UpdatePaymentHistoryAsync(paymentHistory);
                    
                    // Mark reward as processed
                    bool rewardSuccess = rowsAffected > 0;
                    string? rewardError = null;
                    
                    if (!rewardSuccess)
                    {
                        rewardError = "Không thể cộng tiền vào Mem_Account";
                    }
                    
                    await _paymentRepository.MarkRewardAsProcessedAsync(paymentHistory.Id, rewardSuccess, rewardError);
                    
                    _logger.LogInformation(
                        "Đã cập nhật payment history ID: {PaymentHistoryId} với status: paid, paymentId: {PaymentId}",
                        paymentHistory.Id, payment.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Không tìm thấy payment history với CheckoutSessionId: {CheckoutSessionId}",
                        eventAttributes.Data.Id);
                }
            }
            catch (Exception historyEx)
            {
                _logger.LogError(historyEx, "Lỗi khi cập nhật payment history");
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

        // Cập nhật Payment_History
        try
        {
            var paymentHistory = await _paymentRepository.GetPaymentHistoryByCheckoutSessionIdAsync(data.Attributes?.Data?.Id ?? "");
            
            if (paymentHistory != null)
            {
                var payment = checkoutSession?.Payments?.FirstOrDefault();
                
                paymentHistory.Status = "failed";
                paymentHistory.EventType = data.Attributes?.Type;
                paymentHistory.EventId = data.Id;
                paymentHistory.UpdatedAt = DateTime.Now;
                
                if (payment?.Id != null)
                {
                    paymentHistory.PaymentId = payment.Id;
                }
                
                // Get failure information if available
                if (payment?.Attributes != null)
                {
                    paymentHistory.FailureCode = payment.Attributes.Status;
                    paymentHistory.FailureMessage = "Payment failed";
                }

                await _paymentRepository.UpdatePaymentHistoryAsync(paymentHistory);
                
                _logger.LogInformation(
                    "Đã cập nhật payment history ID: {PaymentHistoryId} với status: failed",
                    paymentHistory.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật payment history cho failed payment");
        }
        
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
        await Task.CompletedTask; // Suppress async warning
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
