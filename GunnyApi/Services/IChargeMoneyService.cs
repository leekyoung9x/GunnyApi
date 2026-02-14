using GunnyApi.Models;

namespace GunnyApi.Services;

public interface IChargeMoneyService
{
    /// <summary>
    /// Nạp tiền vào tài khoản game
    /// </summary>
    /// <param name="username">Tên đăng nhập (từ UserContext)</param>
    /// <param name="money">Số tiền nạp</param>
    /// <param name="type">Loại giao dịch</param>
    /// <param name="needMoney">Số tiền thực tế cần thanh toán</param>
    /// <returns>ChargeMoneyResponse</returns>
    Task<ChargeMoneyResponse> ChargeMoneyAsync(string username, int money, string type, decimal needMoney);
    
    /// <summary>
    /// Nạp tiền với ChargeID tùy chỉnh (cho các trường hợp đặc biệt)
    /// </summary>
    Task<ChargeMoneyResponse> ChargeMoneyWithCustomIdAsync(string chargeID, string username, int money, string type, decimal needMoney);
}
