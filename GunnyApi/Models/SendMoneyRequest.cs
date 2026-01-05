namespace GunnyApi.Models;

public class SendMoneyRequest
{
    public string UserName { get; set; } = string.Empty;
    public int Gold { get; set; }
    public int Money { get; set; }
    public int GiftToken { get; set; }
}
