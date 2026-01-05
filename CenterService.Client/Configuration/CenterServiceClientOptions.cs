using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CenterService.Client.Configuration;

/// <summary>
/// Configuration options for CenterService client
/// </summary>
public class CenterServiceClientOptions
{
    /// <summary>
    /// Server address (e.g., "net.tcp://114.29.239.53:2109/" or "http://server:port/service")
    /// </summary>
    public string ServerAddress { get; set; } = "net.tcp://localhost:2109/";

    /// <summary>
    /// Binding type to use
    /// </summary>
    public BindingType BindingType { get; set; } = BindingType.NetTcp;

    /// <summary>
    /// Security mode
    /// </summary>
    public SecurityMode SecurityMode { get; set; } = SecurityMode.None;

    /// <summary>
    /// Maximum received message size in bytes
    /// </summary>
    public long MaxReceivedMessageSize { get; set; } = 65536;

    /// <summary>
    /// Maximum buffer size in bytes
    /// </summary>
    public int MaxBufferSize { get; set; } = 65536;

    /// <summary>
    /// Maximum buffer pool size in bytes
    /// </summary>
    public long MaxBufferPoolSize { get; set; } = 524288;

    /// <summary>
    /// Send timeout
    /// </summary>
    public TimeSpan SendTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Receive timeout
    /// </summary>
    public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Open timeout
    /// </summary>
    public TimeSpan OpenTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Close timeout
    /// </summary>
    public TimeSpan CloseTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Reader quotas max depth
    /// </summary>
    public int ReaderQuotasMaxDepth { get; set; } = 32;

    /// <summary>
    /// Reader quotas max string content length
    /// </summary>
    public int ReaderQuotasMaxStringContentLength { get; set; } = 8192;

    /// <summary>
    /// Reader quotas max array length
    /// </summary>
    public int ReaderQuotasMaxArrayLength { get; set; } = 16384;

    /// <summary>
    /// Reader quotas max bytes per read
    /// </summary>
    public int ReaderQuotasMaxBytesPerRead { get; set; } = 4096;

    /// <summary>
    /// Reader quotas max name table char count
    /// </summary>
    public int ReaderQuotasMaxNameTableCharCount { get; set; } = 16384;

    /// <summary>
    /// Creates a binding based on configuration
    /// </summary>
    public Binding CreateBinding()
    {
        return BindingType switch
        {
            BindingType.NetTcp => CreateNetTcpBinding(),
            BindingType.BasicHttp => CreateBasicHttpBinding(),
            _ => throw new NotSupportedException($"Binding type {BindingType} is not supported")
        };
    }

    private NetTcpBinding CreateNetTcpBinding()
    {
        var binding = new NetTcpBinding(SecurityMode)
        {
            MaxReceivedMessageSize = MaxReceivedMessageSize,
            MaxBufferSize = MaxBufferSize,
            MaxBufferPoolSize = MaxBufferPoolSize,
            SendTimeout = SendTimeout,
            ReceiveTimeout = ReceiveTimeout,
            OpenTimeout = OpenTimeout,
            CloseTimeout = CloseTimeout,
            TransferMode = TransferMode.Buffered
        };

        binding.ReaderQuotas.MaxDepth = ReaderQuotasMaxDepth;
        binding.ReaderQuotas.MaxStringContentLength = ReaderQuotasMaxStringContentLength;
        binding.ReaderQuotas.MaxArrayLength = ReaderQuotasMaxArrayLength;
        binding.ReaderQuotas.MaxBytesPerRead = ReaderQuotasMaxBytesPerRead;
        binding.ReaderQuotas.MaxNameTableCharCount = ReaderQuotasMaxNameTableCharCount;

        if (SecurityMode == SecurityMode.None)
        {
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
        }

        return binding;
    }

    private BasicHttpBinding CreateBasicHttpBinding()
    {
        var binding = new BasicHttpBinding(
            SecurityMode == SecurityMode.None ? BasicHttpSecurityMode.None : BasicHttpSecurityMode.Transport)
        {
            MaxReceivedMessageSize = MaxReceivedMessageSize,
            MaxBufferSize = MaxBufferSize,
            MaxBufferPoolSize = MaxBufferPoolSize,
            SendTimeout = SendTimeout,
            ReceiveTimeout = ReceiveTimeout,
            OpenTimeout = OpenTimeout,
            CloseTimeout = CloseTimeout,
            TransferMode = TransferMode.Buffered,
            MessageEncoding = WSMessageEncoding.Text,
            AllowCookies = false,
            BypassProxyOnLocal = false,
            UseDefaultWebProxy = true
        };

        binding.ReaderQuotas.MaxDepth = ReaderQuotasMaxDepth;
        binding.ReaderQuotas.MaxStringContentLength = ReaderQuotasMaxStringContentLength;
        binding.ReaderQuotas.MaxArrayLength = ReaderQuotasMaxArrayLength;
        binding.ReaderQuotas.MaxBytesPerRead = ReaderQuotasMaxBytesPerRead;
        binding.ReaderQuotas.MaxNameTableCharCount = ReaderQuotasMaxNameTableCharCount;

        return binding;
    }
}

/// <summary>
/// Binding type enumeration
/// </summary>
public enum BindingType
{
    /// <summary>
    /// NetTcp binding (binary, high performance)
    /// </summary>
    NetTcp,

    /// <summary>
    /// BasicHttp binding (SOAP over HTTP)
    /// </summary>
    BasicHttp
}
