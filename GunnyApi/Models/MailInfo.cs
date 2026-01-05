namespace GunnyApi.Models;

public class MailInfo
{
    public int ID { get; set; }
    public string? Annex1 { get; set; }
    public string? Annex2 { get; set; }
    public string? Annex3 { get; set; }
    public string? Annex4 { get; set; }
    public string? Annex5 { get; set; }
    public string? Content { get; set; }
    public int Gold { get; set; }
    public int Money { get; set; }
    public int GiftToken { get; set; }
    public string? Receiver { get; set; }
    public int ReceiverID { get; set; }
    public string? Sender { get; set; }
    public int SenderID { get; set; }
    public string? Title { get; set; }
    public int Type { get; set; }
    public string? Annex1Name { get; set; }
    public string? Annex2Name { get; set; }
    public string? Annex3Name { get; set; }
    public string? Annex4Name { get; set; }
    public string? Annex5Name { get; set; }
    public DateTime? ValidDate { get; set; }
    public string? AnnexRemark { get; set; }
}
