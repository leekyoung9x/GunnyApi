using GunnyApi.Infrastructure.Services;
using GunnyApi.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace GunnyApi.Infrastructure.Middleware;

/// <summary>
/// Middleware to detect and set language from request headers or query string
/// </summary>
public class LanguageDetectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LocalizationSettings _settings;

    public LanguageDetectionMiddleware(RequestDelegate next, IOptions<LocalizationSettings> settings)
    {
        _next = next;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context, ILocalizationService localization)
    {
        string? language = null;

        // Priority 1: Query string (?lang=vi)
        if (_settings.EnableQueryStringDetection && context.Request.Query.ContainsKey("lang"))
        {
            language = context.Request.Query["lang"].ToString();
        }

        // Priority 2: Header (Accept-Language)
        if (string.IsNullOrEmpty(language) && _settings.EnableHeaderDetection)
        {
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                // Parse Accept-Language header (e.g., "vi-VN,vi;q=0.9,en;q=0.8")
                var languages = acceptLanguage.Split(',')
                    .Select(l => l.Split(';')[0].Trim())
                    .Select(l => l.Split('-')[0]) // Get language code only (vi from vi-VN)
                    .FirstOrDefault();
                
                language = languages;
            }
        }

        // Set language if valid
        if (!string.IsNullOrEmpty(language) && _settings.SupportedLanguages.Contains(language))
        {
            localization.SetLanguage(language);
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class LanguageDetectionMiddlewareExtensions
{
    public static IApplicationBuilder UseLanguageDetection(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LanguageDetectionMiddleware>();
    }
}
