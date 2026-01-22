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
    /// Minimum payment amount allowed
    /// </summary>
    public decimal MinimumAmount { get; set; } = 0;

    /// <summary>
    /// Maximum payment amount allowed
    /// </summary>
    public decimal MaximumAmount { get; set; } = 0;
}
