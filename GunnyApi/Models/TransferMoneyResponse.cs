namespace GunnyApi.Models;

public class TransferMoneyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RemainingMemberMoney { get; set; }
    public int TankMoney { get; set; }
}
