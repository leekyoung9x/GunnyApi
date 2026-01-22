namespace GunnyApi.Models;

/// <summary>
/// Represents a payment tier/milestone with amount and rewards
/// </summary>
public class PaymentTier
{
    /// <summary>
    /// Unique identifier for the tier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Payment amount required for this tier (in local currency)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gold reward amount
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    /// Money/Xu reward amount
    /// </summary>
    public int Money { get; set; }

    /// <summary>
    /// Gift Token/Lễ Kim reward amount
    /// </summary>
    public int GiftToken { get; set; }

    /// <summary>
    /// Bonus percentage (optional)
    /// </summary>
    public decimal BonusPercent { get; set; }

    /// <summary>
    /// Display name key for localization (e.g., "PaymentTier.Tier1.Name")
    /// </summary>
    public string? DisplayNameKey { get; set; }

    /// <summary>
    /// Display name for the tier (fallback if no translation)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Description key for localization (e.g., "PaymentTier.Tier1.Description")
    /// </summary>
    public string? DescriptionKey { get; set; }

    /// <summary>
    /// Description of the tier (fallback if no translation)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this tier is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; }
}
