namespace GunnyApi.Infrastructure.Settings;

/// <summary>
/// Configuration settings for localization
/// </summary>
public class LocalizationSettings
{
    /// <summary>
    /// Default language code (e.g., "vi", "en")
    /// </summary>
    public string? DefaultLanguage { get; set; } = "vi";
    
    /// <summary>
    /// Supported language codes
    /// </summary>
    public string[] SupportedLanguages { get; set; } = new[] { "vi", "en" };
    
    /// <summary>
    /// Enable language detection from Accept-Language header
    /// </summary>
    public bool EnableHeaderDetection { get; set; } = true;
    
    /// <summary>
    /// Enable language detection from query string (?lang=vi)
    /// </summary>
    public bool EnableQueryStringDetection { get; set; } = true;
}
