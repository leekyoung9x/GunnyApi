namespace GunnyApi.Infrastructure.Services;

/// <summary>
/// Service for handling localization/internationalization
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get localized string by key and optional parameters
    /// </summary>
    string GetString(string key, params object[] args);
    
    /// <summary>
    /// Set current language
    /// </summary>
    void SetLanguage(string languageCode);
    
    /// <summary>
    /// Get current language
    /// </summary>
    string GetCurrentLanguage();
}
