namespace GunnyApi.Models;

public class PaymentHistory
{
    public int Id { get; set; }
    
    // User Information
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    
    // Payment Information
    public decimal Amount { get; set; }
    public int AmountInCentavos { get; set; }
    public string Currency { get; set; } = "PHP";
    
    // PayMongo Information
    public string? CheckoutSessionId { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? PaymentId { get; set; }
    public string? CheckoutUrl { get; set; }
    
    // Reward Information
    public int? TierId { get; set; }
    public int? MoneyReward { get; set; }
    public int? GoldReward { get; set; }
    public int? GiftTokenReward { get; set; }
    
    // Transaction Status
    public string Status { get; set; } = "pending";
    public string? PaymentMethod { get; set; }
    
    // Description & Metadata
    public string? Description { get; set; }
    public string? ProductName { get; set; }
    public string? Metadata { get; set; }
    
    // Event Information
    public string? EventType { get; set; }
    public string? EventId { get; set; }
    
    // Error Information
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    
    // Reward Status
    public bool IsRewardProcessed { get; set; } = false;
    public DateTime? RewardProcessedAt { get; set; }
    public string? RewardErrorMessage { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class PaymentHistoryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<PaymentHistoryDto>? Data { get; set; }
    public int TotalCount { get; set; }
}

public class PaymentHistoryDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public string? ProductName { get; set; }
    
    // Rewards
    public int? MoneyReward { get; set; }
    public int? GoldReward { get; set; }
    public int? GiftTokenReward { get; set; }
    
    // PayMongo Info
    public string? CheckoutSessionId { get; set; }
    public string? PaymentId { get; set; }
    public string? CheckoutUrl { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    // Status flags
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow && Status == "pending";
    public bool IsPaid => Status == "paid";
    public bool IsFailed => Status == "failed";
}
