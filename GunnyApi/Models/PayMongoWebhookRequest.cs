using System.Text.Json.Serialization;

namespace GunnyApi.Models;

/// <summary>
/// PayMongo Webhook Request - Root object nhận từ PayMongo
/// </summary>
public class PayMongoWebhookRequest
{
    /// <summary>
    /// Dữ liệu event webhook
    /// </summary>
    [JsonPropertyName("data")]
    public PayMongoEventData? Data { get; set; }
}

/// <summary>
/// Event data - Thông tin về event webhook
/// </summary>
public class PayMongoEventData
{
    /// <summary>
    /// ID của event (vd: evt_LrUZABdMHm7hvxF222c4ADp1)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Type luôn là "event"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Thuộc tính của event
    /// </summary>
    [JsonPropertyName("attributes")]
    public PayMongoEventAttributes? Attributes { get; set; }
}

/// <summary>
/// Event attributes - Chi tiết của event
/// </summary>
public class PayMongoEventAttributes
{
    /// <summary>
    /// Loại event (vd: checkout_session.payment.paid, checkout_session.payment.failed)
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Có phải live mode không (false = test mode)
    /// </summary>
    [JsonPropertyName("livemode")]
    public bool Livemode { get; set; }

    /// <summary>
    /// Dữ liệu chi tiết của resource (checkout session, payment, etc)
    /// </summary>
    [JsonPropertyName("data")]
    public PayMongoResourceData? Data { get; set; }

    /// <summary>
    /// Dữ liệu trước khi thay đổi
    /// </summary>
    [JsonPropertyName("previous_data")]
    public Dictionary<string, object>? PreviousData { get; set; }

    /// <summary>
    /// Số lượng webhooks đang chờ gửi
    /// </summary>
    [JsonPropertyName("pending_webhooks")]
    public int PendingWebhooks { get; set; }

    /// <summary>
    /// Thời gian tạo event (Unix timestamp)
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật event (Unix timestamp)
    /// </summary>
    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}

/// <summary>
/// Resource data - Dữ liệu của resource (checkout session, payment)
/// </summary>
public class PayMongoResourceData
{
    /// <summary>
    /// ID của resource (vd: cs_xxx cho checkout session, pay_xxx cho payment)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Type của resource (vd: checkout_session, payment)
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Thuộc tính của resource
    /// </summary>
    [JsonPropertyName("attributes")]
    public PayMongoCheckoutSessionAttributes? Attributes { get; set; }
}

/// <summary>
/// Checkout Session Attributes - Thông tin chi tiết của checkout session
/// </summary>
public class PayMongoCheckoutSessionAttributes
{
    /// <summary>
    /// Thông tin billing
    /// </summary>
    [JsonPropertyName("billing")]
    public PayMongoBilling? Billing { get; set; }

    /// <summary>
    /// Cho phép chỉnh sửa thông tin billing (enabled/disabled)
    /// </summary>
    [JsonPropertyName("billing_information_fields_editable")]
    public string? BillingInformationFieldsEditable { get; set; }

    /// <summary>
    /// URL khi người dùng cancel
    /// </summary>
    [JsonPropertyName("cancel_url")]
    public string? CancelUrl { get; set; }

    /// <summary>
    /// URL của checkout page
    /// </summary>
    [JsonPropertyName("checkout_url")]
    public string? CheckoutUrl { get; set; }

    /// <summary>
    /// Client key để truy cập checkout
    /// </summary>
    [JsonPropertyName("client_key")]
    public string? ClientKey { get; set; }

    /// <summary>
    /// Email của khách hàng
    /// </summary>
    [JsonPropertyName("customer_email")]
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// ID của customer
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; set; }

    /// <summary>
    /// Mô tả checkout session
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Danh sách sản phẩm/dịch vụ
    /// </summary>
    [JsonPropertyName("line_items")]
    public List<PayMongoLineItem>? LineItems { get; set; }

    /// <summary>
    /// Live mode hay test mode
    /// </summary>
    [JsonPropertyName("livemode")]
    public bool Livemode { get; set; }

    /// <summary>
    /// Tên merchant
    /// </summary>
    [JsonPropertyName("merchant")]
    public string? Merchant { get; set; }

    /// <summary>
    /// Thời gian thanh toán thành công (Unix timestamp)
    /// </summary>
    [JsonPropertyName("paid_at")]
    public long? PaidAt { get; set; }

    /// <summary>
    /// Danh sách các payments
    /// </summary>
    [JsonPropertyName("payments")]
    public List<PayMongoPayment>? Payments { get; set; }

    /// <summary>
    /// Payment intent
    /// </summary>
    [JsonPropertyName("payment_intent")]
    public PayMongoPaymentIntent? PaymentIntent { get; set; }

    /// <summary>
    /// Các phương thức thanh toán được hỗ trợ
    /// </summary>
    [JsonPropertyName("payment_method_types")]
    public List<string>? PaymentMethodTypes { get; set; }

    /// <summary>
    /// Phương thức thanh toán đã sử dụng
    /// </summary>
    [JsonPropertyName("payment_method_used")]
    public string? PaymentMethodUsed { get; set; }

    /// <summary>
    /// Số tham chiếu
    /// </summary>
    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Có gửi email receipt không
    /// </summary>
    [JsonPropertyName("send_email_receipt")]
    public bool SendEmailReceipt { get; set; }

    /// <summary>
    /// Hiển thị mô tả
    /// </summary>
    [JsonPropertyName("show_description")]
    public bool ShowDescription { get; set; }

    /// <summary>
    /// Hiển thị line items
    /// </summary>
    [JsonPropertyName("show_line_items")]
    public bool ShowLineItems { get; set; }

    /// <summary>
    /// Trạng thái checkout session
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// URL khi thanh toán thành công
    /// </summary>
    [JsonPropertyName("success_url")]
    public string? SuccessUrl { get; set; }

    /// <summary>
    /// Metadata tùy chỉnh
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Thời gian tạo (Unix timestamp)
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật (Unix timestamp)
    /// </summary>
    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}

/// <summary>
/// Line Item - Sản phẩm/dịch vụ trong đơn hàng
/// </summary>
public class PayMongoLineItem
{
    /// <summary>
    /// Số tiền (tính bằng centavos, vd: 200000 = 2000 PHP)
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ (vd: PHP)
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// Mô tả sản phẩm
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Danh sách hình ảnh
    /// </summary>
    [JsonPropertyName("images")]
    public List<string>? Images { get; set; }

    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Số lượng
    /// </summary>
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

/// <summary>
/// Payment - Chi tiết payment
/// </summary>
public class PayMongoPayment
{
    /// <summary>
    /// ID của payment (vd: pay_xxx)
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Type luôn là "payment"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Thuộc tính của payment
    /// </summary>
    [JsonPropertyName("attributes")]
    public PayMongoPaymentAttributes? Attributes { get; set; }
}

/// <summary>
/// Payment Attributes - Chi tiết thông tin payment
/// </summary>
public class PayMongoPaymentAttributes
{
    /// <summary>
    /// URL để truy cập payment (cho e-wallet)
    /// </summary>
    [JsonPropertyName("access_url")]
    public string? AccessUrl { get; set; }

    /// <summary>
    /// Số tiền (centavos)
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    /// <summary>
    /// ID của balance transaction
    /// </summary>
    [JsonPropertyName("balance_transaction_id")]
    public string? BalanceTransactionId { get; set; }

    /// <summary>
    /// Thông tin billing
    /// </summary>
    [JsonPropertyName("billing")]
    public PayMongoBilling? Billing { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// Mô tả payment
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Số tiền VAT
    /// </summary>
    [JsonPropertyName("digital_withholding_vat_amount")]
    public long DigitalWithholdingVatAmount { get; set; }

    /// <summary>
    /// Có tranh chấp không
    /// </summary>
    [JsonPropertyName("disputed")]
    public bool Disputed { get; set; }

    /// <summary>
    /// Số tham chiếu ngoài
    /// </summary>
    [JsonPropertyName("external_reference_number")]
    public string? ExternalReferenceNumber { get; set; }

    /// <summary>
    /// Phí (centavos)
    /// </summary>
    [JsonPropertyName("fee")]
    public long Fee { get; set; }

    /// <summary>
    /// Phí ngoại tệ
    /// </summary>
    [JsonPropertyName("foreign_fee")]
    public long ForeignFee { get; set; }

    /// <summary>
    /// Instant settlement
    /// </summary>
    [JsonPropertyName("instant_settlement")]
    public object? InstantSettlement { get; set; }

    /// <summary>
    /// Live mode
    /// </summary>
    [JsonPropertyName("livemode")]
    public bool Livemode { get; set; }

    /// <summary>
    /// Số tiền net sau khi trừ phí (centavos)
    /// </summary>
    [JsonPropertyName("net_amount")]
    public long NetAmount { get; set; }

    /// <summary>
    /// Nguồn gốc payment (api, dashboard)
    /// </summary>
    [JsonPropertyName("origin")]
    public string? Origin { get; set; }

    /// <summary>
    /// ID của payment intent
    /// </summary>
    [JsonPropertyName("payment_intent_id")]
    public string? PaymentIntentId { get; set; }

    /// <summary>
    /// Payout info
    /// </summary>
    [JsonPropertyName("payout")]
    public object? Payout { get; set; }

    /// <summary>
    /// Thông tin source (card, e-wallet)
    /// </summary>
    [JsonPropertyName("source")]
    public PayMongoSource? Source { get; set; }

    /// <summary>
    /// Statement descriptor
    /// </summary>
    [JsonPropertyName("statement_descriptor")]
    public string? StatementDescriptor { get; set; }

    /// <summary>
    /// Trạng thái payment (paid, failed, pending)
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Số tiền thuế
    /// </summary>
    [JsonPropertyName("tax_amount")]
    public long? TaxAmount { get; set; }

    /// <summary>
    /// ID của checkout session
    /// </summary>
    [JsonPropertyName("checkout_session_id")]
    public string? CheckoutSessionId { get; set; }

    /// <summary>
    /// Metadata tùy chỉnh
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Promotion info
    /// </summary>
    [JsonPropertyName("promotion")]
    public object? Promotion { get; set; }

    /// <summary>
    /// Danh sách refunds
    /// </summary>
    [JsonPropertyName("refunds")]
    public List<object>? Refunds { get; set; }

    /// <summary>
    /// Danh sách taxes
    /// </summary>
    [JsonPropertyName("taxes")]
    public List<object>? Taxes { get; set; }

    /// <summary>
    /// Thời gian có thể rút tiền (Unix timestamp)
    /// </summary>
    [JsonPropertyName("available_at")]
    public long AvailableAt { get; set; }

    /// <summary>
    /// Thời gian tạo (Unix timestamp)
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// Thời gian credited vào account (Unix timestamp)
    /// </summary>
    [JsonPropertyName("credited_at")]
    public long CreditedAt { get; set; }

    /// <summary>
    /// Thời gian thanh toán (Unix timestamp)
    /// </summary>
    [JsonPropertyName("paid_at")]
    public long? PaidAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật (Unix timestamp)
    /// </summary>
    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}

/// <summary>
/// Payment Intent
/// </summary>
public class PayMongoPaymentIntent
{
    /// <summary>
    /// ID của payment intent
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Type luôn là "payment_intent"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Thuộc tính của payment intent
    /// </summary>
    [JsonPropertyName("attributes")]
    public PayMongoPaymentIntentAttributes? Attributes { get; set; }
}

/// <summary>
/// Payment Intent Attributes
/// </summary>
public class PayMongoPaymentIntentAttributes
{
    /// <summary>
    /// Số tiền
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    /// <summary>
    /// Loại capture (automatic, manual)
    /// </summary>
    [JsonPropertyName("capture_type")]
    public string? CaptureType { get; set; }

    /// <summary>
    /// Client key
    /// </summary>
    [JsonPropertyName("client_key")]
    public string? ClientKey { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// Mô tả
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Live mode
    /// </summary>
    [JsonPropertyName("livemode")]
    public bool Livemode { get; set; }

    /// <summary>
    /// Số tiền gốc
    /// </summary>
    [JsonPropertyName("original_amount")]
    public long OriginalAmount { get; set; }

    /// <summary>
    /// Statement descriptor
    /// </summary>
    [JsonPropertyName("statement_descriptor")]
    public string? StatementDescriptor { get; set; }

    /// <summary>
    /// Trạng thái (awaiting_payment_method, processing, succeeded, etc)
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Lỗi thanh toán gần nhất
    /// </summary>
    [JsonPropertyName("last_payment_error")]
    public object? LastPaymentError { get; set; }

    /// <summary>
    /// Các phương thức thanh toán được phép
    /// </summary>
    [JsonPropertyName("payment_method_allowed")]
    public List<string>? PaymentMethodAllowed { get; set; }

    /// <summary>
    /// Danh sách payments
    /// </summary>
    [JsonPropertyName("payments")]
    public List<PayMongoPayment>? Payments { get; set; }

    /// <summary>
    /// Hành động tiếp theo (cho 3D Secure)
    /// </summary>
    [JsonPropertyName("next_action")]
    public object? NextAction { get; set; }

    /// <summary>
    /// Tùy chọn phương thức thanh toán
    /// </summary>
    [JsonPropertyName("payment_method_options")]
    public PayMongoPaymentMethodOptions? PaymentMethodOptions { get; set; }

    /// <summary>
    /// Metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Setup future usage
    /// </summary>
    [JsonPropertyName("setup_future_usage")]
    public string? SetupFutureUsage { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}

/// <summary>
/// Payment Method Options
/// </summary>
public class PayMongoPaymentMethodOptions
{
    /// <summary>
    /// Tùy chọn cho card
    /// </summary>
    [JsonPropertyName("card")]
    public PayMongoCardOptions? Card { get; set; }
}

/// <summary>
/// Card Options
/// </summary>
public class PayMongoCardOptions
{
    /// <summary>
    /// Yêu cầu 3D Secure (any, automatic)
    /// </summary>
    [JsonPropertyName("request_three_d_secure")]
    public string? RequestThreeDSecure { get; set; }
}

/// <summary>
/// Source - Nguồn thanh toán (card, e-wallet)
/// </summary>
public class PayMongoSource
{
    /// <summary>
    /// ID của source
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Type của source (card, gcash, grab_pay, etc)
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Brand của card (visa, mastercard)
    /// </summary>
    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    /// <summary>
    /// Quốc gia phát hành card
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// 4 số cuối của card
    /// </summary>
    [JsonPropertyName("last4")]
    public string? Last4 { get; set; }
}

/// <summary>
/// Billing - Thông tin billing
/// </summary>
public class PayMongoBilling
{
    /// <summary>
    /// Tên người thanh toán
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Địa chỉ
    /// </summary>
    [JsonPropertyName("address")]
    public PayMongoAddress? Address { get; set; }
}

/// <summary>
/// Address - Địa chỉ
/// </summary>
public class PayMongoAddress
{
    /// <summary>
    /// Địa chỉ dòng 1
    /// </summary>
    [JsonPropertyName("line1")]
    public string? Line1 { get; set; }

    /// <summary>
    /// Địa chỉ dòng 2
    /// </summary>
    [JsonPropertyName("line2")]
    public string? Line2 { get; set; }

    /// <summary>
    /// Thành phố
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Tỉnh/Tiểu bang
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Mã bưu điện
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Quốc gia
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
