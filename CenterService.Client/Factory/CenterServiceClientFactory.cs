using System.ServiceModel;
using CenterService.Client.Configuration;

namespace CenterService.Client.Factory;

/// <summary>
/// Factory for creating CenterService client instances
/// </summary>
public static class CenterServiceClientFactory
{
    /// <summary>
    /// Creates a client with default options
    /// </summary>
    public static CenterServiceClient CreateClient()
    {
        var options = new CenterServiceClientOptions();
        return CreateClient(options);
    }

    /// <summary>
    /// Creates a client with custom options
    /// </summary>
    /// <param name="options">Client configuration options</param>
    public static CenterServiceClient CreateClient(CenterServiceClientOptions options)
    {
        var binding = options.CreateBinding();
        var endpoint = new EndpointAddress(options.ServerAddress);
        return new CenterServiceClient(binding, endpoint);
    }

    /// <summary>
    /// Creates a client with server address string
    /// </summary>
    /// <param name="serverAddress">Server address (e.g., "net.tcp://114.29.239.53:2109/")</param>
    public static CenterServiceClient CreateClient(string serverAddress)
    {
        var options = new CenterServiceClientOptions
        {
            ServerAddress = serverAddress,
            BindingType = serverAddress.StartsWith("net.tcp://", StringComparison.OrdinalIgnoreCase) 
                ? BindingType.NetTcp 
                : BindingType.BasicHttp
        };
        return CreateClient(options);
    }

    /// <summary>
    /// Creates a NetTcp client with specific address
    /// </summary>
    /// <param name="serverAddress">Server address</param>
    /// <param name="securityMode">Security mode</param>
    public static CenterServiceClient CreateNetTcpClient(string serverAddress, SecurityMode securityMode = SecurityMode.None)
    {
        var options = new CenterServiceClientOptions
        {
            ServerAddress = serverAddress,
            BindingType = BindingType.NetTcp,
            SecurityMode = securityMode
        };
        return CreateClient(options);
    }

    /// <summary>
    /// Creates a BasicHttp client with specific address
    /// </summary>
    /// <param name="serverAddress">Server address</param>
    /// <param name="securityMode">Security mode</param>
    public static CenterServiceClient CreateBasicHttpClient(string serverAddress, SecurityMode securityMode = SecurityMode.None)
    {
        var options = new CenterServiceClientOptions
        {
            ServerAddress = serverAddress,
            BindingType = BindingType.BasicHttp,
            SecurityMode = securityMode
        };
        return CreateClient(options);
    }
}
