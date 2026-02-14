namespace GunnyApi.Models;

public class ChargeMoneyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? RequestUrl { get; set; }
}
