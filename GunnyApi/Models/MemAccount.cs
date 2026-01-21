namespace GunnyApi.Models
{
    public class MemAccount
    {
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string? Phone { get; set; }
        public int Money { get; set; }
        public int MoneyLock { get; set; }
        public int TotalMoney { get; set; }
        public int MoneyEvent { get; set; }
        public int Point { get; set; }
        public int CountLucky { get; set; }
        public int VIPLevel { get; set; }
        public int VIPExp { get; set; }
        public bool IsBan { get; set; }
        public string? IPCreate { get; set; }
        public bool AllowSocialLogin { get; set; }
        public int? TimeCreate { get; set; }
        public string? Pass2 { get; set; }
        public bool? TwoFactorStatus { get; set; }
        public bool? VerifiedEmail { get; set; }
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiresAt { get; set; }
        public string? ActiveIP { get; set; }
    }
}
