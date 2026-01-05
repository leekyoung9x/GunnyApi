namespace GunnyApi.Models;

public class PlayerInfo
{
    public int UserID { get; set; }
    public string? NickName { get; set; }
    public string? UserName { get; set; }
    public int Money { get; set; }
    public int Gold { get; set; }
    public int GiftToken { get; set; }
    // Add other properties as needed
}
