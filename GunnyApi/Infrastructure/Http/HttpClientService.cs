using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GunnyApi.Infrastructure.Http;

/// <summary>
/// Service để xử lý HTTP requests
/// </summary>
public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpClientService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpClientService(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Gửi GET request
    /// </summary>
    public async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
    {
        try
        {
            var client = CreateClient(headers);
            _logger.LogInformation("GET Request to: {Url}", url);

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET Request failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, content);
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi GET request và deserialize sang object
    /// </summary>
    public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null) where T : class
    {
        var content = await GetAsync(url, headers);
        
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing response from {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi POST request với body
    /// </summary>
    public async Task<string> PostAsync(string url, object? body = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            var client = CreateClient(headers);
            _logger.LogInformation("POST Request to: {Url}", url);

            HttpContent? content = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("POST Request failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);
            }

            return responseContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi POST request và deserialize response sang object
    /// </summary>
    public async Task<T?> PostAsync<T>(string url, object? body = null, Dictionary<string, string>? headers = null) where T : class
    {
        var content = await PostAsync(url, body, headers);
        
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing response from {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi PUT request với body
    /// </summary>
    public async Task<string> PutAsync(string url, object? body = null, Dictionary<string, string>? headers = null)
    {
        try
        {
            var client = CreateClient(headers);
            _logger.LogInformation("PUT Request to: {Url}", url);

            HttpContent? content = null;
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await client.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PUT Request failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);
            }

            return responseContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi PUT request và deserialize response sang object
    /// </summary>
    public async Task<T?> PutAsync<T>(string url, object? body = null, Dictionary<string, string>? headers = null) where T : class
    {
        var content = await PutAsync(url, body, headers);
        
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing response from {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi DELETE request
    /// </summary>
    public async Task<string> DeleteAsync(string url, Dictionary<string, string>? headers = null)
    {
        try
        {
            var client = CreateClient(headers);
            _logger.LogInformation("DELETE Request to: {Url}", url);

            var response = await client.DeleteAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DELETE Request failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, content);
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi DELETE request và deserialize response sang object
    /// </summary>
    public async Task<T?> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null) where T : class
    {
        var content = await DeleteAsync(url, headers);
        
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing response from {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gửi POST request với form data
    /// </summary>
    public async Task<string> PostFormAsync(string url, Dictionary<string, string> formData, Dictionary<string, string>? headers = null)
    {
        try
        {
            var client = CreateClient(headers);
            _logger.LogInformation("POST Form Request to: {Url}", url);

            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("POST Form Request failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);
            }

            return responseContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST Form request to {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Download file từ URL
    /// </summary>
    public async Task<byte[]> DownloadFileAsync(string url, Dictionary<string, string>? headers = null)
    {
        try
        {
            var client = CreateClient(headers);
            _logger.LogInformation("Download File from: {Url}", url);

            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Download failed. Status: {Status}", response.StatusCode);
                throw new HttpRequestException($"Download failed with status code {response.StatusCode}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Tạo HttpClient với headers tùy chỉnh
    /// </summary>
    private HttpClient CreateClient(Dictionary<string, string>? headers = null)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        // Thêm default headers
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Thêm custom headers
        if (headers != null)
        {
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return client;
    }
}
