namespace GunnyApi.Infrastructure.Settings;

/// <summary>
/// Kestrel server configuration settings
/// </summary>
public class KestrelSettings
{
    /// <summary>
    /// HTTP port number
    /// </summary>
    public int HttpPort { get; set; } = 5176;

    /// <summary>
    /// HTTPS port number
    /// </summary>
    public int HttpsPort { get; set; } = 7062;

    /// <summary>
    /// Enable or disable HTTPS
    /// </summary>
    public bool EnableHttps { get; set; } = true;

    /// <summary>
    /// Listen on all network interfaces (0.0.0.0) or only localhost (127.0.0.1)
    /// </summary>
    public bool ListenOnAllInterfaces { get; set; } = true;
}
