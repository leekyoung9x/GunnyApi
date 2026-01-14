namespace GunnyApi.Models
{
    public class ServerInfo
    {
        public string AreaId { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public string LoadGameUrl { get; set; } = string.Empty;
        public string ConfigUrl { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
    }
}
