namespace GunnyApi.Infrastructure.Http;

/// <summary>
/// Interface cho HTTP Client Service
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Gửi GET request
    /// </summary>
    Task<string> GetAsync(string url, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gửi GET request và deserialize sang object
    /// </summary>
    Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null) where T : class;

    /// <summary>
    /// Gửi POST request với body
    /// </summary>
    Task<string> PostAsync(string url, object? body = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gửi POST request và deserialize response sang object
    /// </summary>
    Task<T?> PostAsync<T>(string url, object? body = null, Dictionary<string, string>? headers = null) where T : class;

    /// <summary>
    /// Gửi PUT request với body
    /// </summary>
    Task<string> PutAsync(string url, object? body = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gửi PUT request và deserialize response sang object
    /// </summary>
    Task<T?> PutAsync<T>(string url, object? body = null, Dictionary<string, string>? headers = null) where T : class;

    /// <summary>
    /// Gửi DELETE request
    /// </summary>
    Task<string> DeleteAsync(string url, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gửi DELETE request và deserialize response sang object
    /// </summary>
    Task<T?> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null) where T : class;

    /// <summary>
    /// Gửi POST request với form data
    /// </summary>
    Task<string> PostFormAsync(string url, Dictionary<string, string> formData, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Download file từ URL
    /// </summary>
    Task<byte[]> DownloadFileAsync(string url, Dictionary<string, string>? headers = null);
}
