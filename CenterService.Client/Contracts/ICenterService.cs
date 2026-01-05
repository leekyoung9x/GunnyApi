using System.ServiceModel;

namespace CenterService.Client.Contracts;

/// <summary>
/// Center Service contract interface
/// </summary>
[ServiceContract(ConfigurationName = "CenterService.ICenterService")]
public interface ICenterService
{
    /// <summary>
    /// Get list of available servers
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/GetServerList", 
        ReplyAction = "http://tempuri.org/ICenterService/GetServerListResponse")]
    Models.ServerData[] GetServerList();

    /// <summary>
    /// Charge money for a user
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/ChargeMoney", 
        ReplyAction = "http://tempuri.org/ICenterService/ChargeMoneyResponse")]
    bool ChargeMoney(int userID, string chargeID);

    /// <summary>
    /// Send system notice
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/SystemNotice", 
        ReplyAction = "http://tempuri.org/ICenterService/SystemNoticeResponse")]
    bool SystemNotice(string msg);

    /// <summary>
    /// Kick off user from server
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/KitoffUser", 
        ReplyAction = "http://tempuri.org/ICenterService/KitoffUserResponse")]
    bool KitoffUser(int playerID, string msg);

    /// <summary>
    /// Reload server list
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/ReLoadServerList", 
        ReplyAction = "http://tempuri.org/ICenterService/ReLoadServerListResponse")]
    bool ReLoadServerList();

    /// <summary>
    /// Send mail notification to player
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/MailNotice", 
        ReplyAction = "http://tempuri.org/ICenterService/MailNoticeResponse")]
    bool MailNotice(int playerID);

    /// <summary>
    /// Activate or deactivate player
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/ActivePlayer", 
        ReplyAction = "http://tempuri.org/ICenterService/ActivePlayerResponse")]
    bool ActivePlayer(bool isActive);

    /// <summary>
    /// Create new player
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/CreatePlayer", 
        ReplyAction = "http://tempuri.org/ICenterService/CreatePlayerResponse")]
    bool CreatePlayer(int id, string name, string password, bool isFirst);

    /// <summary>
    /// Validate login and get user ID
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/ValidateLoginAndGetID", 
        ReplyAction = "http://tempuri.org/ICenterService/ValidateLoginAndGetIDResponse")]
    bool ValidateLoginAndGetID(string name, string password, ref int userID, ref bool isFirst);

    /// <summary>
    /// Update AAS state
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/AASUpdateState", 
        ReplyAction = "http://tempuri.org/ICenterService/AASUpdateStateResponse")]
    bool AASUpdateState(bool state);

    /// <summary>
    /// Get AAS state
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/AASGetState", 
        ReplyAction = "http://tempuri.org/ICenterService/AASGetStateResponse")]
    int AASGetState();

    /// <summary>
    /// Update experience rate
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/ExperienceRateUpdate", 
        ReplyAction = "http://tempuri.org/ICenterService/ExperienceRateUpdateResponse")]
    int ExperienceRateUpdate(int serverId);

    /// <summary>
    /// Update server notice
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/NoticeServerUpdate", 
        ReplyAction = "http://tempuri.org/ICenterService/NoticeServerUpdateResponse")]
    int NoticeServerUpdate(int serverId, int type);

    /// <summary>
    /// Update config state
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/UpdateConfigState", 
        ReplyAction = "http://tempuri.org/ICenterService/UpdateConfigStateResponse")]
    bool UpdateConfigState(int type, bool state);

    /// <summary>
    /// Get config state
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/GetConfigState", 
        ReplyAction = "http://tempuri.org/ICenterService/GetConfigStateResponse")]
    int GetConfigState(int type);

    /// <summary>
    /// Reload configuration
    /// </summary>
    [OperationContract(Action = "http://tempuri.org/ICenterService/Reload", 
        ReplyAction = "http://tempuri.org/ICenterService/ReloadResponse")]
    bool Reload(string type);
}
