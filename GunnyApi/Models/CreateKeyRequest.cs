namespace GunnyApi.Models
{
    public class CreateKeyRequest
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string areaId { get; set; } = string.Empty;
    }
}
