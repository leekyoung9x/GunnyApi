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
        _currentLanguage = _settings.DefaultLanguage ?? "en";
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
            ["User.IdMismatch"] = "ID không khớp",
            ["User.UpdateSuccess"] = "Cập nhật thành công",
            ["User.DeleteSuccess"] = "Xóa thành công",
            ["User.Unauthorized"] = "Không xác định được người dùng",
            ["User.UsernameRequiredNotEmpty"] = "Tên tài khoản không được để trống",
            ["User.AmountMustBePositive"] = "Số tiền phải lớn hơn 0",
            ["User.ConnectionError"] = "Lỗi kết nối: {0}",
            
            // Login messages
            ["Login.Success"] = "Đăng nhập thành công",
            ["Login.Failed"] = "Có lỗi xảy ra khi đăng nhập: {0}",
            ["Login.UsernameRequired"] = "Username không được rỗng",
            ["Login.PasswordRequired"] = "Password không được rỗng",
            ["Login.Unauthorized"] = "Không tìm thấy thông tin user từ token",
            ["Login.InvalidUsername"] = "Vui lòng nhập tài khoản",
            ["Login.InvalidPassword"] = "Vui lòng nhập đầy đủ thông tin",
            ["Login.AuthFailed"] = "Đăng nhập thất bại",
            ["Login.GameServerError"] = "Không thể kết nối game server: {0}",
            ["Login.ServerError"] = "Có lỗi xảy ra: {0}",
            
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
            ["Server.LoginFailed"] = "Đăng nhập game thất bại: {0}",
            ["Server.LoginError"] = "Có lỗi xảy ra khi đăng nhập game: {0}",
            ["Server.CreateKeySuccess"] = "Tạo key thành công",
            ["Server.CreateKeyError"] = "Có lỗi xảy ra khi tạo key: {0}",
            ["Server.UsernameRequired"] = "Username không được để trống",
            ["Server.LoginGameError"] = "Có lỗi xảy ra: {0}",
            
            // Payment messages
            ["Payment.CreateCheckoutSuccess"] = "Tạo phiên thanh toán thành công",
            ["Payment.CreateCheckoutError"] = "Có lỗi xảy ra khi tạo phiên thanh toán: {0}",
            ["Payment.WebhookProcessSuccess"] = "Xử lý webhook thành công",
            ["Payment.WebhookProcessError"] = "Có lỗi xảy ra khi xử lý webhook: {0}",
            ["Payment.InvalidRequest"] = "Dữ liệu webhook không hợp lệ",
            ["Payment.InvalidSignature"] = "Chữ ký webhook không hợp lệ",
            ["Payment.Unauthorized"] = "Không thể xác thực người dùng",
            ["Payment.ConfigNotSetup"] = "Cấu hình thanh toán chưa được thiết lập",
            ["Payment.CannotCreateSession"] = "Không thể tạo phiên thanh toán",
            ["Payment.InvalidResponse"] = "Phản hồi từ cổng thanh toán không hợp lệ",
            ["Payment.PaymentFor"] = "Thanh toán cho user: {0}",
            ["Payment.TopupFor"] = "Nạp tiền vào tài khoản {0}",
            
            // Payment Tiers
            ["PaymentTier.Tier1.Name"] = "Gói Khởi Đầu",
            ["PaymentTier.Tier1.Description"] = "Gói nạp cơ bản cho người mới",
            ["PaymentTier.Tier2.Name"] = "Gói Tiết Kiệm",
            ["PaymentTier.Tier2.Description"] = "Nhận thêm 10% bonus và 50 Lễ Kim",
            ["PaymentTier.Tier3.Name"] = "Gói Phổ Biến",
            ["PaymentTier.Tier3.Description"] = "Nhận thêm 20% bonus và 150 Lễ Kim",
            ["PaymentTier.Tier4.Name"] = "Gói Cao Cấp",
            ["PaymentTier.Tier4.Description"] = "Nhận thêm 30% bonus và 500 Lễ Kim",
            ["PaymentTier.Tier5.Name"] = "Gói VIP",
            ["PaymentTier.Tier5.Description"] = "Nhận thêm 40% bonus và 1500 Lễ Kim",
            ["PaymentTier.Tier6.Name"] = "Gói Đại Gia",
            ["PaymentTier.Tier6.Description"] = "Nhận thêm 50% bonus và 4000 Lễ Kim"
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
            ["User.IdMismatch"] = "ID mismatch",
            ["User.UpdateSuccess"] = "Update successful",
            ["User.DeleteSuccess"] = "Delete successful",
            ["User.Unauthorized"] = "Unable to identify user",
            ["User.UsernameRequiredNotEmpty"] = "Username cannot be empty",
            ["User.AmountMustBePositive"] = "Amount must be greater than 0",
            ["User.ConnectionError"] = "Connection error: {0}",
            
            // Login messages
            ["Login.Success"] = "Login successful",
            ["Login.Failed"] = "An error occurred during login: {0}",
            ["Login.UsernameRequired"] = "Username is required",
            ["Login.PasswordRequired"] = "Password is required",
            ["Login.Unauthorized"] = "User information not found in token",
            ["Login.InvalidUsername"] = "Please enter your username",
            ["Login.InvalidPassword"] = "Please enter all required information",
            ["Login.AuthFailed"] = "Login failed",
            ["Login.GameServerError"] = "Unable to connect to game server: {0}",
            ["Login.ServerError"] = "An error occurred: {0}",
            
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
            ["Server.LoginFailed"] = "Game login failed: {0}",
            ["Server.LoginError"] = "An error occurred during game login: {0}",
            ["Server.CreateKeySuccess"] = "Key created successfully",
            ["Server.CreateKeyError"] = "An error occurred while creating key: {0}",
            ["Server.UsernameRequired"] = "Username is required",
            ["Server.LoginGameError"] = "An error occurred: {0}",
            
            // Payment messages
            ["Payment.CreateCheckoutSuccess"] = "Checkout session created successfully",
            ["Payment.CreateCheckoutError"] = "An error occurred while creating checkout session: {0}",
            ["Payment.WebhookProcessSuccess"] = "Webhook processed successfully",
            ["Payment.WebhookProcessError"] = "An error occurred while processing webhook: {0}",
            ["Payment.InvalidRequest"] = "Invalid webhook data",
            ["Payment.InvalidSignature"] = "Invalid webhook signature",
            ["Payment.Unauthorized"] = "Unable to authenticate user",
            ["Payment.ConfigNotSetup"] = "Payment configuration not set up",
            ["Payment.CannotCreateSession"] = "Unable to create payment session",
            ["Payment.InvalidResponse"] = "Invalid response from payment gateway",
            ["Payment.PaymentFor"] = "Payment for user: {0}",
            ["Payment.TopupFor"] = "Top-up for account {0}",
            
            // Payment Tiers
            ["PaymentTier.Tier1.Name"] = "Starter Pack",
            ["PaymentTier.Tier1.Description"] = "Basic top-up package for new players",
            ["PaymentTier.Tier2.Name"] = "Value Pack",
            ["PaymentTier.Tier2.Description"] = "Get extra 10% bonus and 50 Gift Tokens",
            ["PaymentTier.Tier3.Name"] = "Popular Pack",
            ["PaymentTier.Tier3.Description"] = "Get extra 20% bonus and 150 Gift Tokens",
            ["PaymentTier.Tier4.Name"] = "Premium Pack",
            ["PaymentTier.Tier4.Description"] = "Get extra 30% bonus and 500 Gift Tokens",
            ["PaymentTier.Tier5.Name"] = "VIP Pack",
            ["PaymentTier.Tier5.Description"] = "Get extra 40% bonus and 1500 Gift Tokens",
            ["PaymentTier.Tier6.Name"] = "Tycoon Pack",
            ["PaymentTier.Tier6.Description"] = "Get extra 50% bonus and 4000 Gift Tokens"
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
            _translations.TryGetValue(_settings.DefaultLanguage ?? "en", out var defaultDict))
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
