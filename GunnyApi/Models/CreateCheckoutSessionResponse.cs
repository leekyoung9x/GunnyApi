namespace GunnyApi.Models;

/// <summary>
/// Response trả về cho client
/// </summary>
public class CreateCheckoutSessionResponse
{
    public string CheckoutSessionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string ClientKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Response từ PayMongo API - Sử dụng lại các class đã có trong PayMongoWebhookRequest.cs
/// </summary>
public class PayMongoCheckoutSessionResponse
{
    public PayMongoCheckoutSessionData Data { get; set; } = new();
}

public class PayMongoCheckoutSessionData
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public PayMongoCheckoutSessionAttributes Attributes { get; set; } = new();
}
