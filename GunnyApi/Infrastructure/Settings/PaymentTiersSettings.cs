using GunnyApi.Models;

namespace GunnyApi.Infrastructure.Settings;

/// <summary>
/// Configuration settings for payment tiers/milestones
/// </summary>
public class PaymentTiersSettings
{
    /// <summary>
    /// List of available payment tiers
    /// </summary>
    public List<PaymentTier> Tiers { get; set; } = new();

    /// <summary>
    /// Currency code (e.g., "PHP", "VND", "USD")
    /// </summary>
    public string Currency { get; set; } = "PHP";

    /// <summary>
    /// Whether payment tiers are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether to add Gold to Tank database when payment is successful
    /// </summary>
    public bool EnableGoldReward { get; set; } = false;

    /// <summary>
    /// Whether to add GiftToken to Tank database when payment is successful
    /// </summary>
    public bool EnableGiftTokenReward { get; set; } = false;

    /// <summary>
    /// Minimum payment amount allowed
    /// </summary>
    public decimal MinimumAmount { get; set; } = 0;

    /// <summary>
    /// Maximum payment amount allowed
    /// </summary>
    public decimal MaximumAmount { get; set; } = 0;
}
