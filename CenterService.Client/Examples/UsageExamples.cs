using CenterService.Client.Configuration;
using CenterService.Client.Factory;

namespace CenterService.Client.Examples;

/// <summary>
/// Example usage demonstrations
/// </summary>
public static class UsageExamples
{
    /// <summary>
    /// Example 1: Simple NetTcp client usage
    /// </summary>
    public static void SimpleNetTcpExample()
    {
        Console.WriteLine("=== Example 1: Simple NetTcp Client ===");
        
        // Create client
        using var client = CenterServiceClientFactory.CreateNetTcpClient("net.tcp://114.29.239.53:2109/");

        try
        {
            // Send mail notice
            int playerId = 6727;
            bool result = client.MailNotice(playerId);
            Console.WriteLine($"MailNotice({playerId}): {result}");

            // Get server list
            var servers = client.GetServerList();
            Console.WriteLine($"\nFound {servers.Length} servers:");
            foreach (var server in servers)
            {
                Console.WriteLine($"  - {server.Name} ({server.Ip}:{server.Port}) - Online: {server.Online}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 2: Using custom configuration
    /// </summary>
    public static void CustomConfigurationExample()
    {
        Console.WriteLine("\n=== Example 2: Custom Configuration ===");

        var options = new CenterServiceClientOptions
        {
            ServerAddress = "net.tcp://114.29.239.53:2109/",
            BindingType = BindingType.NetTcp,
            SecurityMode = System.ServiceModel.SecurityMode.None,
            MaxReceivedMessageSize = 65536,
            ReceiveTimeout = TimeSpan.FromMinutes(10),
            SendTimeout = TimeSpan.FromMinutes(1),
            MaxBufferSize = 65536
        };

        using var client = CenterServiceClientFactory.CreateClient(options);

        try
        {
            // Get AAS state
            int state = client.AASGetState();
            Console.WriteLine($"AAS State: {state}");

            // System notice
            bool noticeResult = client.SystemNotice("Test notification from .NET 8 client");
            Console.WriteLine($"System Notice sent: {noticeResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 3: Player management operations
    /// </summary>
    public static void PlayerManagementExample()
    {
        Console.WriteLine("\n=== Example 3: Player Management ===");

        using var client = CenterServiceClientFactory.CreateNetTcpClient("net.tcp://114.29.239.53:2109/");

        try
        {
            // Create player
            bool createResult = client.CreatePlayer(
                id: 99999,
                name: "TestPlayer",
                password: "testpass123",
                isFirst: true
            );
            Console.WriteLine($"Create Player: {createResult}");

            // Validate login
            int userId = 0;
            bool isFirst = false;
            bool loginResult = client.ValidateLoginAndGetID(
                name: "TestPlayer",
                password: "testpass123",
                ref userId,
                ref isFirst
            );
            Console.WriteLine($"Login validated: {loginResult}, UserID: {userId}, IsFirst: {isFirst}");

            // Activate player
            bool activateResult = client.ActivePlayer(true);
            Console.WriteLine($"Activate Player: {activateResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 4: Server administration operations
    /// </summary>
    public static void ServerAdministrationExample()
    {
        Console.WriteLine("\n=== Example 4: Server Administration ===");

        using var client = CenterServiceClientFactory.CreateNetTcpClient("net.tcp://114.29.239.53:2109/");

        try
        {
            // Reload server list
            bool reloadResult = client.ReLoadServerList();
            Console.WriteLine($"Reload Server List: {reloadResult}");

            // Update experience rate
            int expRate = client.ExperienceRateUpdate(serverId: 1);
            Console.WriteLine($"Experience Rate for Server 1: {expRate}");

            // Get config state
            int configState = client.GetConfigState(type: 1);
            Console.WriteLine($"Config State (type 1): {configState}");

            // Update config state
            bool updateConfigResult = client.UpdateConfigState(type: 1, state: true);
            Console.WriteLine($"Update Config State: {updateConfigResult}");

            // Reload configuration
            bool reloadConfigResult = client.Reload(type: "all");
            Console.WriteLine($"Reload Config: {reloadConfigResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 5: BasicHttp binding (for Postman testing)
    /// </summary>
    public static void BasicHttpExample()
    {
        Console.WriteLine("\n=== Example 5: BasicHttp Client (Testable with Postman) ===");

        // Note: Server must support HTTP endpoint for this to work
        using var client = CenterServiceClientFactory.CreateBasicHttpClient("http://114.29.239.53:8080/CenterService");

        try
        {
            int playerId = 12345;
            bool result = client.MailNotice(playerId);
            Console.WriteLine($"MailNotice via HTTP: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Note: Server must have HTTP endpoint configured");
        }
    }

    /// <summary>
    /// Example 6: Error handling and retry pattern
    /// </summary>
    public static async Task ErrorHandlingExample()
    {
        Console.WriteLine("\n=== Example 6: Error Handling with Retry ===");

        const int maxRetries = 3;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            attempt++;
            Console.WriteLine($"Attempt {attempt}/{maxRetries}...");

            try
            {
                using var client = CenterServiceClientFactory.CreateNetTcpClient("net.tcp://114.29.239.53:2109/");
                
                bool result = client.MailNotice(12345);
                Console.WriteLine($"Success! Result: {result}");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                
                if (attempt < maxRetries)
                {
                    Console.WriteLine("Waiting 2 seconds before retry...");
                    await Task.Delay(2000);
                }
                else
                {
                    Console.WriteLine("Max retries reached. Operation failed.");
                }
            }
        }
    }

    /// <summary>
    /// Run all examples
    /// </summary>
    public static async Task RunAllExamples()
    {
        Console.WriteLine("??????????????????????????????????????????????????????????");
        Console.WriteLine("?  CenterService Client .NET 8 - Usage Examples         ?");
        Console.WriteLine("??????????????????????????????????????????????????????????\n");

        try
        {
            SimpleNetTcpExample();
            CustomConfigurationExample();
            PlayerManagementExample();
            ServerAdministrationExample();
            BasicHttpExample();
            await ErrorHandlingExample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nGlobal Error: {ex.Message}");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Examples completed!");
    }
}
