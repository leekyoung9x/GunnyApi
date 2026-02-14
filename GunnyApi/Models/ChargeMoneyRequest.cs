namespace GunnyApi.Models;

public class ChargeMoneyRequest
{
    public int Money { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal NeedMoney { get; set; }
}
