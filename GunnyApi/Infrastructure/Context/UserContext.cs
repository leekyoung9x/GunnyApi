namespace GunnyApi.Infrastructure.Context;

/// <summary>
/// Context chứa thông tin user hiện tại trong request
/// </summary>
public class UserContext : IUserContext
{
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}

/// <summary>
/// Interface cho UserContext để dễ dàng inject và test
/// </summary>
public interface IUserContext
{
    int? UserId { get; set; }
    string? Username { get; set; }
    string? Email { get; set; }
    string? FullName { get; set; }
    bool IsAuthenticated { get; }
}
