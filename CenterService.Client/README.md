# CenterService Client for .NET 8

A modern .NET 8 WCF client library for CenterService using CoreWCF compatibility packages.

## Features

- ? Full .NET 8 support
- ? NetTcp and BasicHttp bindings
- ? Easy configuration via code or options
- ? Factory pattern for client creation
- ? Proper disposal handling
- ? XML documentation
- ? Reusable as DLL across multiple projects

## Installation

### Build the library
```bash
dotnet build CenterService.Client.Net8.csproj -c Release
```

The compiled DLL will be in: `bin/Release/net8.0/CenterService.Client.Net8.dll`

## Usage Examples

### 1. Basic Usage with Factory (Recommended)

```csharp
using CenterService.Client.Net8.Factory;

// Create client with NetTcp binding
using var client = CenterServiceClientFactory.CreateNetTcpClient("net.tcp://114.29.239.53:2109/");

// Call service methods
bool result = client.MailNotice(12345);
Console.WriteLine($"MailNotice result: {result}");

var servers = client.GetServerList();
foreach (var server in servers)
{
    Console.WriteLine($"Server: {server.Name}, Online: {server.Online}");
}
```

### 2. Using Custom Configuration

```csharp
using CenterService.Client.Net8.Configuration;
using CenterService.Client.Net8.Factory;

var options = new CenterServiceClientOptions
{
    ServerAddress = "net.tcp://114.29.239.53:2109/",
    BindingType = BindingType.NetTcp,
    SecurityMode = System.ServiceModel.SecurityMode.None,
    MaxReceivedMessageSize = 65536,
    ReceiveTimeout = TimeSpan.FromMinutes(10),
    SendTimeout = TimeSpan.FromMinutes(1)
};

using var client = CenterServiceClientFactory.CreateClient(options);
bool result = client.MailNotice(12345);
```

### 3. Direct Instantiation

```csharp
using System.ServiceModel;
using CenterService.Client.Net8;

var binding = new NetTcpBinding(SecurityMode.None)
{
    MaxReceivedMessageSize = 65536,
    MaxBufferSize = 65536
};

var endpoint = new EndpointAddress("net.tcp://114.29.239.53:2109/");
using var client = new CenterServiceClient(binding, endpoint);

bool result = client.MailNotice(12345);
```

### 4. Using in ASP.NET Core with Dependency Injection

```csharp
// Program.cs or Startup.cs
builder.Services.AddSingleton(sp =>
{
    return CenterServiceClientFactory.CreateNetTcpClient("net.tcp://114.29.239.53:2109/");
});

// In your controller or service
public class MyService
{
    private readonly CenterServiceClient _centerClient;

    public MyService(CenterServiceClient centerClient)
    {
        _centerClient = centerClient;
    }

    public async Task<bool> SendMailNotification(int playerId)
    {
        return _centerClient.MailNotice(playerId);
    }
}
```

## Available Methods

| Method | Description |
|--------|-------------|
| `GetServerList()` | Get list of available servers |
| `ChargeMoney(int userID, string chargeID)` | Charge money for a user |
| `SystemNotice(string msg)` | Send system notice |
| `KitoffUser(int playerID, string msg)` | Kick off user from server |
| `ReLoadServerList()` | Reload server list |
| `MailNotice(int playerID)` | Send mail notification to player |
| `ActivePlayer(bool isActive)` | Activate or deactivate player |
| `CreatePlayer(int id, string name, string password, bool isFirst)` | Create new player |
| `ValidateLoginAndGetID(string name, string password, ref int userID, ref bool isFirst)` | Validate login and get user ID |
| `AASUpdateState(bool state)` | Update AAS state |
| `AASGetState()` | Get AAS state |
| `ExperienceRateUpdate(int serverId)` | Update experience rate |
| `NoticeServerUpdate(int serverId, int type)` | Update server notice |
| `UpdateConfigState(int type, bool state)` | Update config state |
| `GetConfigState(int type)` | Get config state |
| `Reload(string type)` | Reload configuration |

## Configuration Options

### NetTcp Binding (Default)
- Protocol: `net.tcp://`
- High performance binary protocol
- Best for server-to-server communication

### BasicHttp Binding
- Protocol: `http://` or `https://`
- SOAP over HTTP
- Better for cross-platform scenarios
- Can be tested with Postman

## Testing with Postman (BasicHttp only)

If you configure the service to use BasicHttp, you can test with Postman:

```
POST http://your-server:port/CenterService
Content-Type: text/xml; charset=utf-8
SOAPAction: "http://tempuri.org/ICenterService/MailNotice"

<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <MailNotice xmlns="http://tempuri.org/">
      <playerID>12345</playerID>
    </MailNotice>
  </soap:Body>
</soap:Envelope>
```

## Requirements

- .NET 8.0 or higher
- System.ServiceModel.* packages (included)

## License

[Your License Here]

## Support

For issues and questions, please contact [your contact info]
