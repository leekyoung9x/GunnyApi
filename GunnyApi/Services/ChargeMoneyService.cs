using GunnyApi.Infrastructure.Http;
using GunnyApi.Infrastructure.Services;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Infrastructure.Utils;
using GunnyApi.Models;
using GunnyApi.Repositories;
using Microsoft.Extensions.Options;
using System.Web;

namespace GunnyApi.Services;

public class ChargeMoneyService : IChargeMoneyService
{
    private readonly GameSettings _gameSettings;
    private readonly IHttpClientService _httpClientService;
    private readonly ILocalizationService _localization;
    private readonly IUserRepository _userRepository;

    public ChargeMoneyService(
        IOptions<GameSettings> gameSettings,
        IHttpClientService httpClientService,
        ILocalizationService localization,
        IUserRepository userRepository)
    {
        _gameSettings = gameSettings.Value;
        _httpClientService = httpClientService;
        _localization = localization;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Nạp tiền vào tài khoản game với ChargeID tự động (GUID)
    /// </summary>
    public async Task<ChargeMoneyResponse> ChargeMoneyAsync(
        string username,
        int money, 
        string type, 
        decimal needMoney)
    {
        // Tạo ChargeID tự động bằng GUID
        string chargeID = Guid.NewGuid().ToString("N"); // Format: 32 hex characters without dashes
        
        return await ChargeMoneyWithCustomIdAsync(chargeID, username, money, type, needMoney);
    }

    /// <summary>
    /// Nạp tiền với ChargeID tùy chỉnh (internal - được gọi từ ChargeMoneyAsync)
    /// </summary>
    public async Task<ChargeMoneyResponse> ChargeMoneyWithCustomIdAsync(
        string chargeID,
        string username,
        int money, 
        string type, 
        decimal needMoney)
    {
        // Lấy player info từ username
        var playerInfo = await _userRepository.GetPlayerByUserNameAsync(username);
        if (playerInfo == null)
        {
            return new ChargeMoneyResponse
            {
                Success = false,
                Message = _localization.GetString("Server.PlayerNotFound")
            };
        }

        int userID = playerInfo.UserID;
        try
        {
            // Validate inputs
            if (string.IsNullOrEmpty(chargeID))
            {
                return new ChargeMoneyResponse
                {
                    Success = false,
                    Message = _localization.GetString("Server.ChargeIDRequired")
                };
            }

            if (string.IsNullOrEmpty(username))
            {
                return new ChargeMoneyResponse
                {
                    Success = false,
                    Message = _localization.GetString("Server.UsernameRequired")
                };
            }

            if (money <= 0)
            {
                return new ChargeMoneyResponse
                {
                    Success = false,
                    Message = _localization.GetString("Server.InvalidAmount")
                };
            }

            if (userID <= 0)
            {
                return new ChargeMoneyResponse
                {
                    Success = false,
                    Message = _localization.GetString("Server.UserIDRequired")
                };
            }

            // Lấy charge key từ config
            string key = string.IsNullOrEmpty(_gameSettings.ChargeKey) 
                ? _gameSettings.LoginKey 
                : _gameSettings.ChargeKey;

            // Tạo verification hash: md5(chargeID + username + money + type + needMoney + key)
            string verificationHash = MD5Helper.ToMD5(
                chargeID + 
                username + 
                money.ToString() + 
                type + 
                needMoney.ToString() + 
                key
            );

            // Tạo content để gửi đến game server: chargeID|username|money|type|needMoney|hash
            string content = $"{chargeID}|{username}|{money}|{type}|{needMoney}|{verificationHash}";
            string encodedContent = HttpUtility.UrlEncode(content);

            // Lấy site từ config
            string site = string.IsNullOrEmpty(_gameSettings.Site) ? "" : _gameSettings.Site.ToLower();
            
            // Tạo URL với đầy đủ các tham số: content, site, nickname
            string chargeUrl = $"{_gameSettings.BaseRequestUrl}ChargeMoney.aspx?content={encodedContent}&site={HttpUtility.UrlEncode(site)}&nickname={userID}";
            
            // Gọi game server
            string result = await RequestContent(chargeUrl);

            if (result == "0") // Charge thành công
            {
                return new ChargeMoneyResponse
                {
                    Success = true,
                    Message = _localization.GetString("Server.ChargeSuccess"),
                    Content = content,
                    RequestUrl = chargeUrl
                };
            }
            else
            {
                return new ChargeMoneyResponse
                {
                    Success = false,
                    Message = _localization.GetString("Server.ChargeFailed", result),
                    Content = content
                };
            }
        }
        catch (Exception ex)
        {
            return new ChargeMoneyResponse
            {
                Success = false,
                Message = _localization.GetString("Server.ChargeError", ex.Message)
            };
        }
    }

    /// <summary>
    /// Gửi HTTP request và nhận response
    /// </summary>
    private async Task<string> RequestContent(string url)
    {
        try
        {
            return await _httpClientService.GetAsync(url);
        }
        catch (Exception ex)
        {
            return _localization.GetString("User.ConnectionError", ex.Message);
        }
    }
}
