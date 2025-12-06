using GunnyApi.Models;

namespace GunnyApi.Infrastructure.Security;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    int? ValidateToken(string token);
}
