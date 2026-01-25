using Dapper;
using GunnyApi.Infrastructure.Database;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Models;

namespace GunnyApi.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PaymentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreatePaymentHistoryAsync(PaymentHistory payment)
    {
        // Validate inputs
        SqlInjectionProtection.ValidateInputs(
            (payment.Username, nameof(payment.Username)),
            (payment.Currency, nameof(payment.Currency))
        );

        if (!string.IsNullOrEmpty(payment.CheckoutSessionId))
        {
            SqlInjectionProtection.ValidateInput(payment.CheckoutSessionId, nameof(payment.CheckoutSessionId));
        }

        var sql = @"
            INSERT INTO Payment_History (
                UserId, Username, Amount, AmountInCentavos, Currency,
                CheckoutSessionId, CheckoutUrl, TierId,
                MoneyReward, GoldReward, GiftTokenReward,
                Status, Description, ProductName, Metadata,
                CreatedAt, ExpiresAt
            )
            VALUES (
                @UserId, @Username, @Amount, @AmountInCentavos, @Currency,
                @CheckoutSessionId, @CheckoutUrl, @TierId,
                @MoneyReward, @GoldReward, @GiftTokenReward,
                @Status, @Description, @ProductName, @Metadata,
                @CreatedAt, @ExpiresAt
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, payment);
    }

    public async Task<bool> UpdatePaymentHistoryAsync(PaymentHistory payment)
    {
        var sql = @"
            UPDATE Payment_History SET
                PaymentIntentId = @PaymentIntentId,
                PaymentId = @PaymentId,
                Status = @Status,
                PaymentMethod = @PaymentMethod,
                EventType = @EventType,
                EventId = @EventId,
                FailureCode = @FailureCode,
                FailureMessage = @FailureMessage,
                UpdatedAt = @UpdatedAt,
                PaidAt = @PaidAt,
                Metadata = @Metadata
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, payment);
        return affectedRows > 0;
    }

    public async Task<PaymentHistory?> GetPaymentHistoryByIdAsync(int id)
    {
        var sql = "SELECT * FROM Payment_History WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PaymentHistory>(sql, new { Id = id });
    }

    public async Task<PaymentHistory?> GetPaymentHistoryByCheckoutSessionIdAsync(string checkoutSessionId)
    {
        // Validate input
        SqlInjectionProtection.ValidateInput(checkoutSessionId, nameof(checkoutSessionId));

        var sql = "SELECT * FROM Payment_History WHERE CheckoutSessionId = @CheckoutSessionId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PaymentHistory>(sql, new { CheckoutSessionId = checkoutSessionId });
    }

    public async Task<PaymentHistory?> GetPaymentHistoryByPaymentIdAsync(string paymentId)
    {
        // Validate input
        SqlInjectionProtection.ValidateInput(paymentId, nameof(paymentId));

        var sql = "SELECT * FROM Payment_History WHERE PaymentId = @PaymentId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PaymentHistory>(sql, new { PaymentId = paymentId });
    }

    public async Task<IEnumerable<PaymentHistory>> GetPaymentHistoriesByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 20)
    {
        var offset = (pageNumber - 1) * pageSize;

        var sql = @"
            SELECT * FROM Payment_History 
            WHERE UserId = @UserId 
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PaymentHistory>(sql, new 
        { 
            UserId = userId, 
            Offset = offset, 
            PageSize = pageSize 
        });
    }

    public async Task<int> GetPaymentHistoryCountByUserIdAsync(int userId)
    {
        var sql = "SELECT COUNT(*) FROM Payment_History WHERE UserId = @UserId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<PaymentHistory>> GetPendingPaymentsAsync()
    {
        var sql = @"
            SELECT * FROM Payment_History 
            WHERE Status = 'pending' 
            AND ExpiresAt > GETDATE()
            ORDER BY CreatedAt DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PaymentHistory>(sql);
    }

    public async Task<bool> MarkRewardAsProcessedAsync(int paymentHistoryId, bool success, string? errorMessage = null)
    {
        var sql = @"
            UPDATE Payment_History SET
                IsRewardProcessed = @IsRewardProcessed,
                RewardProcessedAt = @RewardProcessedAt,
                RewardErrorMessage = @RewardErrorMessage,
                UpdatedAt = GETDATE()
            WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            Id = paymentHistoryId,
            IsRewardProcessed = success,
            RewardProcessedAt = success ? DateTime.Now : (DateTime?)null,
            RewardErrorMessage = errorMessage
        });

        return affectedRows > 0;
    }

    public async Task<bool> UpdatePaymentStatusAsync(string checkoutSessionId, string status, DateTime? expiredAt = null)
    {
        // Validate inputs
        SqlInjectionProtection.ValidateInputs(
            (checkoutSessionId, nameof(checkoutSessionId)),
            (status, nameof(status))
        );

        var sql = @"
            UPDATE Payment_History SET
                Status = @Status,
                ExpiresAt = COALESCE(@ExpiresAt, ExpiresAt),
                UpdatedAt = GETDATE()
            WHERE CheckoutSessionId = @CheckoutSessionId";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            CheckoutSessionId = checkoutSessionId,
            Status = status,
            ExpiresAt = expiredAt
        });

        return affectedRows > 0;
    }
}
