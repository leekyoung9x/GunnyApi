namespace GunnyApi.Models;

/// <summary>
/// Model mẫu kế thừa từ BaseRequest
/// Các request model khác có thể kế thừa từ BaseRequest để tự động có UserId
/// </summary>
public class SampleRequest : BaseRequest
{
    public string? Data { get; set; }
    // Thêm các thuộc tính khác tùy theo nhu cầu
}
