namespace GunnyApi.Models;

/// <summary>
/// Request để tạo Checkout Session từ client
/// </summary>
public class CreateCheckoutSessionRequest
{
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Model gửi tới PayMongo API
/// </summary>
public class PayMongoCheckoutSessionRequest
{
    public PayMongoCheckoutData Data { get; set; } = new();
}

public class PayMongoCheckoutData
{
    public PayMongoCheckoutAttributes Attributes { get; set; } = new();
}

public class PayMongoCheckoutAttributes
{
    public bool SendEmailReceipt { get; set; }
    public bool ShowDescription { get; set; }
    public bool ShowLineItems { get; set; }
    public List<string> PaymentMethodTypes { get; set; } = new();
    public List<PayMongoLineItem> LineItems { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}
