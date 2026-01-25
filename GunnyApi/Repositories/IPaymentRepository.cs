using GunnyApi.Models;

namespace GunnyApi.Repositories;

public interface IPaymentRepository
{
    Task<int> CreatePaymentHistoryAsync(PaymentHistory payment);
    Task<bool> UpdatePaymentHistoryAsync(PaymentHistory payment);
    Task<PaymentHistory?> GetPaymentHistoryByIdAsync(int id);
    Task<PaymentHistory?> GetPaymentHistoryByCheckoutSessionIdAsync(string checkoutSessionId);
    Task<PaymentHistory?> GetPaymentHistoryByPaymentIdAsync(string paymentId);
    Task<IEnumerable<PaymentHistory>> GetPaymentHistoriesByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 20);
    Task<int> GetPaymentHistoryCountByUserIdAsync(int userId);
    Task<IEnumerable<PaymentHistory>> GetPendingPaymentsAsync();
    Task<bool> MarkRewardAsProcessedAsync(int paymentHistoryId, bool success, string? errorMessage = null);
    Task<bool> UpdatePaymentStatusAsync(string checkoutSessionId, string status, DateTime? expiredAt = null);
}
