using System.ServiceModel;
using System.ServiceModel.Channels;
using CenterService.Client.Contracts;

namespace CenterService.Client;

/// <summary>
/// WCF Client for Center Service using .NET 8
/// </summary>
public class CenterServiceClient : ClientBase<ICenterService>, ICenterService, IDisposable
{
    /// <summary>
    /// Initializes a new instance with default binding and endpoint
    /// </summary>
    public CenterServiceClient() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with custom binding and endpoint
    /// </summary>
    /// <param name="binding">WCF binding configuration</param>
    /// <param name="remoteAddress">Remote endpoint address</param>
    public CenterServiceClient(Binding binding, EndpointAddress remoteAddress)
        : base(binding, remoteAddress)
    {
    }

    /// <inheritdoc />
    public Models.ServerData[] GetServerList()
    {
        return Channel.GetServerList();
    }

    /// <inheritdoc />
    public bool ChargeMoney(int userID, string chargeID)
    {
        return Channel.ChargeMoney(userID, chargeID);
    }

    /// <inheritdoc />
    public bool SystemNotice(string msg)
    {
        return Channel.SystemNotice(msg);
    }

    /// <inheritdoc />
    public bool KitoffUser(int playerID, string msg)
    {
        return Channel.KitoffUser(playerID, msg);
    }

    /// <inheritdoc />
    public bool ReLoadServerList()
    {
        return Channel.ReLoadServerList();
    }

    /// <inheritdoc />
    public bool MailNotice(int playerID)
    {
        return Channel.MailNotice(playerID);
    }

    /// <inheritdoc />
    public bool ActivePlayer(bool isActive)
    {
        return Channel.ActivePlayer(isActive);
    }

    /// <inheritdoc />
    public bool CreatePlayer(int id, string name, string password, bool isFirst)
    {
        return Channel.CreatePlayer(id, name, password, isFirst);
    }

    /// <inheritdoc />
    public bool ValidateLoginAndGetID(string name, string password, ref int userID, ref bool isFirst)
    {
        return Channel.ValidateLoginAndGetID(name, password, ref userID, ref isFirst);
    }

    /// <inheritdoc />
    public bool AASUpdateState(bool state)
    {
        return Channel.AASUpdateState(state);
    }

    /// <inheritdoc />
    public int AASGetState()
    {
        return Channel.AASGetState();
    }

    /// <inheritdoc />
    public int ExperienceRateUpdate(int serverId)
    {
        return Channel.ExperienceRateUpdate(serverId);
    }

    /// <inheritdoc />
    public int NoticeServerUpdate(int serverId, int type)
    {
        return Channel.NoticeServerUpdate(serverId, type);
    }

    /// <inheritdoc />
    public bool UpdateConfigState(int type, bool state)
    {
        return Channel.UpdateConfigState(type, state);
    }

    /// <inheritdoc />
    public int GetConfigState(int type)
    {
        return Channel.GetConfigState(type);
    }

    /// <inheritdoc />
    public bool Reload(string type)
    {
        return Channel.Reload(type);
    }

    /// <summary>
    /// Dispose the client properly
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (State == CommunicationState.Faulted)
            {
                Abort();
            }
            else
            {
                Close();
            }
        }
        catch
        {
            Abort();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
