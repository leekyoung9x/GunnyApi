using Microsoft.Extensions.Options;
using GunnyApi.Infrastructure.Settings;

namespace GunnyApi.Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private string _currentLanguage;
    private readonly LocalizationSettings _settings;

    public LocalizationService(IOptions<LocalizationSettings> settings)
    {
        _settings = settings.Value;
        _currentLanguage = _settings.DefaultLanguage ?? "vi";
        _translations = new Dictionary<string, Dictionary<string, string>>();
        
        LoadTranslations();
    }

    private void LoadTranslations()
    {
        // Vietnamese translations
        _translations["vi"] = new Dictionary<string, string>
        {
            // Common messages
            ["Error.Generic"] = "Có lỗi xảy ra",
            ["Error.NotFound"] = "Không tìm thấy",
            ["Success.Generic"] = "Thành công",
            
            // User messages
            ["User.NotFound"] = "Không tìm thấy user",
            ["User.UsernameExists"] = "Username '{0}' đã tồn tại",
            ["User.EmailExists"] = "Email '{0}' đã tồn tại",
            ["User.UsernameRequired"] = "Username không được rỗng",
            ["User.EmailRequired"] = "Email không được rỗng",
            ["User.PasswordRequired"] = "Password không được rỗng",
            ["User.AccountNotFound"] = "Tài khoản <strong>{0}</strong> không tồn tại.",
            
            // Login messages
            ["Login.Success"] = "Đăng nhập thành công",
            ["Login.Failed"] = "Có lỗi xảy ra khi đăng nhập: {0}",
            ["Login.UsernameRequired"] = "Username không được rỗng",
            ["Login.PasswordRequired"] = "Password không được rỗng",
            ["Login.Unauthorized"] = "Không tìm thấy thông tin user từ token",
            
            // Register messages
            ["Register.Success"] = "Đăng ký tài khoản thành công",
            ["Register.EmailExistsOrError"] = "Email đã tồn tại hoặc có lỗi khi tạo tài khoản",
            ["Register.AccountCreatedButDetailError"] = "Đã tạo tài khoản nhưng có lỗi khi tạo thông tin chi tiết người chơi",
            
            // Money transfer messages
            ["Money.TransferSuccess"] = "Chuyển {0} Xu thành công từ Member sang Tank qua mail. {1}",
            ["Money.TransferSuccessDetail"] = "Đã chuyển thành công <br /> {0} Xu <br /> {1} Vàng <br /> {2} Lễ kim <br /> cho tài khoản <strong>{3}</strong>{4}",
            ["Money.MailError"] = " (Lỗi thông báo: {0})",
            ["Money.ErrorOccurred"] = "Có lỗi xảy ra: {0}",
            
            // Server messages
            ["Server.NotFoundOrError"] = "Không tìm thấy server hoặc có lỗi xảy ra: {0}",
            ["Server.GetListSuccess"] = "Lấy danh sách server thành công",
            ["Server.GetListError"] = "Có lỗi xảy ra khi lấy danh sách server: {0}",
            ["Server.LoginSuccess"] = "Đăng nhập game thành công",
            ["Server.LoginError"] = "Có lỗi xảy ra khi đăng nhập game: {0}",
            ["Server.CreateKeySuccess"] = "Tạo key thành công",
            ["Server.CreateKeyError"] = "Có lỗi xảy ra khi tạo key: {0}",
            
            // Payment messages
            ["Payment.CreateCheckoutSuccess"] = "Tạo phiên thanh toán thành công",
            ["Payment.CreateCheckoutError"] = "Có lỗi xảy ra khi tạo phiên thanh toán: {0}",
            ["Payment.WebhookProcessSuccess"] = "Xử lý webhook thành công",
            ["Payment.WebhookProcessError"] = "Có lỗi xảy ra khi xử lý webhook: {0}",
            ["Payment.InvalidRequest"] = "Dữ liệu webhook không hợp lệ",
            ["Payment.InvalidSignature"] = "Chữ ký webhook không hợp lệ",
            
            // Password change messages
            ["Password.OldPasswordRequired"] = "Mật khẩu cũ không được rỗng",
            ["Password.NewPasswordRequired"] = "Mật khẩu mới không được rỗng",
            ["Password.ConfirmPasswordRequired"] = "Xác nhận mật khẩu không được rỗng",
            ["Password.PasswordsDoNotMatch"] = "Mật khẩu mới và xác nhận mật khẩu không khớp",
            ["Password.NewPasswordSameAsOld"] = "Mật khẩu mới không được trùng với mật khẩu cũ",
            ["Password.ChangeSuccess"] = "Đổi mật khẩu thành công",
            ["Password.OldPasswordIncorrect"] = "Mật khẩu cũ không đúng",
            ["Password.ChangeFailed"] = "Có lỗi xảy ra khi đổi mật khẩu: {0}"
        };

        // English translations
        _translations["en"] = new Dictionary<string, string>
        {
            // Common messages
            ["Error.Generic"] = "An error occurred",
            ["Error.NotFound"] = "Not found",
            ["Success.Generic"] = "Success",
            
            // User messages
            ["User.NotFound"] = "User not found",
            ["User.UsernameExists"] = "Username '{0}' already exists",
            ["User.EmailExists"] = "Email '{0}' already exists",
            ["User.UsernameRequired"] = "Username is required",
            ["User.EmailRequired"] = "Email is required",
            ["User.PasswordRequired"] = "Password is required",
            ["User.AccountNotFound"] = "Account <strong>{0}</strong> does not exist.",
            
            // Login messages
            ["Login.Success"] = "Login successful",
            ["Login.Failed"] = "An error occurred during login: {0}",
            ["Login.UsernameRequired"] = "Username is required",
            ["Login.PasswordRequired"] = "Password is required",
            ["Login.Unauthorized"] = "User information not found in token",
            
            // Register messages
            ["Register.Success"] = "Account registration successful",
            ["Register.EmailExistsOrError"] = "Email already exists or an error occurred while creating the account",
            ["Register.AccountCreatedButDetailError"] = "Account created but an error occurred while creating player details",
            
            // Money transfer messages
            ["Money.TransferSuccess"] = "Successfully transferred {0} Xu from Member to Tank via mail. {1}",
            ["Money.TransferSuccessDetail"] = "Successfully transferred <br /> {0} Xu <br /> {1} Gold <br /> {2} Gift Token <br /> to account <strong>{3}</strong>{4}",
            ["Money.MailError"] = " (Notification error: {0})",
            ["Money.ErrorOccurred"] = "An error occurred: {0}",
            
            // Server messages
            ["Server.NotFoundOrError"] = "Server not found or an error occurred: {0}",
            ["Server.GetListSuccess"] = "Server list retrieved successfully",
            ["Server.GetListError"] = "An error occurred while retrieving server list: {0}",
            ["Server.LoginSuccess"] = "Game login successful",
            ["Server.LoginError"] = "An error occurred during game login: {0}",
            ["Server.CreateKeySuccess"] = "Key created successfully",
            ["Server.CreateKeyError"] = "An error occurred while creating key: {0}",
            
            // Payment messages
            ["Payment.CreateCheckoutSuccess"] = "Checkout session created successfully",
            ["Payment.CreateCheckoutError"] = "An error occurred while creating checkout session: {0}",
            ["Payment.WebhookProcessSuccess"] = "Webhook processed successfully",
            ["Payment.WebhookProcessError"] = "An error occurred while processing webhook: {0}",
            ["Payment.InvalidRequest"] = "Invalid webhook data",
            ["Payment.InvalidSignature"] = "Invalid webhook signature",
            
            // Password change messages
            ["Password.OldPasswordRequired"] = "Old password is required",
            ["Password.NewPasswordRequired"] = "New password is required",
            ["Password.ConfirmPasswordRequired"] = "Confirm password is required",
            ["Password.PasswordsDoNotMatch"] = "New password and confirm password do not match",
            ["Password.NewPasswordSameAsOld"] = "New password must be different from old password",
            ["Password.ChangeSuccess"] = "Password changed successfully",
            ["Password.OldPasswordIncorrect"] = "Old password is incorrect",
            ["Password.ChangeFailed"] = "An error occurred while changing password: {0}"
        };
    }

    public string GetString(string key, params object[] args)
    {
        if (_translations.TryGetValue(_currentLanguage, out var languageDict))
        {
            if (languageDict.TryGetValue(key, out var value))
            {
                return args.Length > 0 ? string.Format(value, args) : value;
            }
        }
        
        // Fallback to default language if current language doesn't have the key
        if (_currentLanguage != _settings.DefaultLanguage && 
            _translations.TryGetValue(_settings.DefaultLanguage ?? "vi", out var defaultDict))
        {
            if (defaultDict.TryGetValue(key, out var defaultValue))
            {
                return args.Length > 0 ? string.Format(defaultValue, args) : defaultValue;
            }
        }
        
        // Return key if translation not found
        return key;
    }

    public void SetLanguage(string languageCode)
    {
        if (_translations.ContainsKey(languageCode))
        {
            _currentLanguage = languageCode;
        }
    }

    public string GetCurrentLanguage()
    {
        return _currentLanguage;
    }
}
