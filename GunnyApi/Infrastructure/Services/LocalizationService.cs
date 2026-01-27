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
            
            // Password change messages
            ["Password.OldPasswordRequired"] = "Mật khẩu cũ không được rỗng",
            ["Password.NewPasswordRequired"] = "Mật khẩu mới không được rỗng",
            ["Password.ConfirmPasswordRequired"] = "Xác nhận mật khẩu không được rỗng",
            ["Password.PasswordsDoNotMatch"] = "Mật khẩu mới và xác nhận mật khẩu không khớp",
            ["Password.NewPasswordSameAsOld"] = "Mật khẩu mới không được trùng với mật khẩu cũ",
            ["Password.ChangeSuccess"] = "Đổi mật khẩu thành công",
            ["Password.OldPasswordIncorrect"] = "Mật khẩu cũ không đúng",
            ["Password.ChangeFailed"] = "Có lỗi xảy ra khi đổi mật khẩu: {0}",
            ["Password.Required"] = "Mật khẩu không được để trống",
            ["Password.TooShort"] = "Mật khẩu phải có ít nhất 6 ký tự",
            ["Payment.InvalidSignature"] = "Chữ ký webhook không hợp lệ",
            ["Payment.Unauthorized"] = "Không thể xác thực người dùng",
            
            // Change Email messages
            ["ChangeEmail.AllFieldsRequired"] = "Vui lòng điền đầy đủ thông tin",
            ["ChangeEmail.InvalidEmailFormat"] = "Email không đúng định dạng",
            ["ChangeEmail.EmailsDoNotMatch"] = "Email mới và xác nhận email không khớp",
            ["ChangeEmail.SameAsCurrentEmail"] = "Email mới không được trùng với email hiện tại",
            ["ChangeEmail.EmailAlreadyExists"] = "Email này đã được sử dụng bởi tài khoản khác",
            ["ChangeEmail.NewEmailRequired"] = "Vui lòng cung cấp email mới",
            ["ChangeEmail.OtpCreated"] = "Đã tạo mã OTP",
            ["ChangeEmail.OtpCreationFailed"] = "Không thể tạo mã OTP",
            ["ChangeEmail.OtpSentToCurrentEmail"] = "Mã OTP đã được gửi đến email hiện tại của bạn",
            ["ChangeEmail.OtpRequired"] = "Vui lòng nhập mã OTP",
            ["ChangeEmail.OtpVerified"] = "Xác thực OTP thành công",
            ["ChangeEmail.OtpSentToNewEmail"] = "Mã OTP đã được gửi đến email mới của bạn",
            ["ChangeEmail.EmailMismatch"] = "Email không khớp",
            ["ChangeEmail.UpdateFailed"] = "Không thể cập nhật email",
            ["ChangeEmail.Success"] = "Đổi email thành công! Vui lòng đăng nhập lại với email mới.",
            ["ChangeEmail.InvalidStep"] = "Bước xác thực không hợp lệ",
            ["ChangeEmail.NoActiveRequest"] = "Không tìm thấy yêu cầu đổi email nào",
            ["ChangeEmail.OtpResentToCurrentEmail"] = "Đã gửi lại mã OTP đến email hiện tại",
            ["ChangeEmail.OtpResentToNewEmail"] = "Đã gửi lại mã OTP đến email mới",
            
            // Change Email - Old Email OTP Template
            ["ChangeEmail.OldEmail.Subject"] = "Xác nhận đổi email - Bước 1",
            ["ChangeEmail.OldEmail.Title"] = "Xác Nhận Đổi Email",
            ["ChangeEmail.OldEmail.Greeting"] = "Xin chào {0}!",
            ["ChangeEmail.OldEmail.Intro"] = "Bạn đã yêu cầu thay đổi email bảo mật cho tài khoản Gunny Game của mình. Để xác nhận đây là bạn, vui lòng sử dụng mã OTP bên dưới:",
            ["ChangeEmail.OldEmail.OtpLabel"] = "Mã OTP của bạn:",
            ["ChangeEmail.OldEmail.Validity"] = "Mã này có hiệu lực trong 15 phút",
            ["ChangeEmail.OldEmail.WarningTitle"] = "Cảnh báo bảo mật",
            ["ChangeEmail.OldEmail.Warning"] = "Nếu bạn không yêu cầu thay đổi email, vui lòng bỏ qua email này và đổi mật khẩu ngay lập tức để bảo vệ tài khoản.",
            ["ChangeEmail.OldEmail.Ignore"] = "Sau khi xác nhận email cũ, bạn sẽ nhận được mã OTP khác ở email mới để hoàn tất quá trình.",
            ["ChangeEmail.OldEmail.AutoMessage"] = "Email này được gửi tự động, vui lòng không trả lời.",
            ["ChangeEmail.OldEmail.Copyright"] = "&copy; 2026 Gunny Game. All rights reserved.",
            
            // Change Email - New Email OTP Template
            ["ChangeEmail.NewEmail.Subject"] = "Xác nhận email mới - Bước 2",
            ["ChangeEmail.NewEmail.Title"] = "Xác Nhận Email Mới",
            ["ChangeEmail.NewEmail.Greeting"] = "Xin chào {0}!",
            ["ChangeEmail.NewEmail.Intro"] = "Bạn đã xác nhận thành công email cũ. Bây giờ, vui lòng xác nhận email mới này để hoàn tất quá trình thay đổi email bảo mật:",
            ["ChangeEmail.NewEmail.OtpLabel"] = "Mã OTP của bạn:",
            ["ChangeEmail.NewEmail.Validity"] = "Mã này có hiệu lực trong 15 phút",
            ["ChangeEmail.NewEmail.WarningTitle"] = "Lưu ý quan trọng",
            ["ChangeEmail.NewEmail.Warning"] = "Sau khi xác nhận, email này sẽ trở thành email bảo mật chính thức của tài khoản. Bạn sẽ cần đăng nhập lại với email mới.",
            ["ChangeEmail.NewEmail.Ignore"] = "Nếu bạn không thực hiện thao tác này, vui lòng bỏ qua email này.",
            ["ChangeEmail.NewEmail.AutoMessage"] = "Email này được gửi tự động, vui lòng không trả lời.",
            ["ChangeEmail.NewEmail.Copyright"] = "&copy; 2026 Gunny Game. All rights reserved.",
            
            // Forgot Password Email Template
            ["ForgotPassword.EmailSubject"] = "Đặt lại mật khẩu - Gunny Game",
            ["ForgotPassword.EmailTitle"] = "🔐 Đặt lại mật khẩu",
            ["ForgotPassword.EmailGreeting"] = "Xin chào {0}!",
            ["ForgotPassword.EmailIntro"] = "Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Gunny Game của mình.",
            ["ForgotPassword.EmailClickButton"] = "Nhấn vào nút bên dưới để đặt lại mật khẩu:",
            ["ForgotPassword.EmailButtonText"] = "Đặt lại mật khẩu",
            ["ForgotPassword.EmailOrCopyLink"] = "Hoặc copy link sau vào trình duyệt:",
            ["ForgotPassword.EmailWarningTitle"] = "⚠️ Lưu ý:",
            ["ForgotPassword.EmailWarning1"] = "Link này sẽ hết hạn sau <strong>1 giờ</strong>",
            ["ForgotPassword.EmailWarning2"] = "Chỉ sử dụng được <strong>1 lần</strong>",
            ["ForgotPassword.EmailWarning3"] = "Không chia sẻ link này với bất kỳ ai",
            ["ForgotPassword.EmailIgnore"] = "Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này. Mật khẩu của bạn sẽ không thay đổi.",
            ["ForgotPassword.EmailAutoMessage"] = "Email này được gửi tự động, vui lòng không trả lời.",
            ["ForgotPassword.EmailCopyright"] = "&copy; 2026 Gunny Game. All rights reserved.",
            ["ForgotPassword.EmailSent"] = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu đến email của bạn.",
            ["ForgotPassword.ProcessError"] = "Lỗi khi xử lý yêu cầu: {0}",
            ["User.EmailInvalid"] = "Email không hợp lệ",
            
            // Reset Password messages
            ["ResetPassword.TokenInvalid"] = "Token không hợp lệ",
            ["ResetPassword.TokenExpired"] = "Token không hợp lệ hoặc đã hết hạn",
            ["ResetPassword.TokenValid"] = "Token hợp lệ",
            ["ResetPassword.VerifyTokenError"] = "Lỗi khi xác thực token: {0}",
            ["ResetPassword.Success"] = "Đặt lại mật khẩu thành công!",
            ["ResetPassword.Failed"] = "Không thể đặt lại mật khẩu. Vui lòng thử lại.",
            ["ResetPassword.Error"] = "Lỗi khi đặt lại mật khẩu: {0}",
            
            // Profile update messages
            ["Profile.UsernameRequired"] = "Username không được rỗng",
            ["Profile.NicknameRequired"] = "Nickname không được rỗng",
            ["Profile.InvalidEmailFormat"] = "Email không đúng định dạng",
            ["Profile.UsernameLengthInvalid"] = "Username phải có độ dài từ 5-100 ký tự",
            ["Profile.NicknameLengthInvalid"] = "Nickname phải có độ dài từ 3-20 ký tự",
            ["Profile.NicknameInvalidCharacters"] = "Nickname chỉ được chứa chữ cái, số, khoảng trắng, gạch dưới và gạch ngang",
            ["Profile.NoChangesDetected"] = "Không có thay đổi nào được phát hiện",
            ["Profile.UsernameAlreadyExists"] = "Username '{0}' đã tồn tại",
            ["Profile.NicknameAlreadyExists"] = "Nickname '{0}' đã tồn tại",
            ["Profile.UpdateUsernameFailed"] = "Không thể cập nhật username",
            ["Profile.UpdateNicknameFailed"] = "Không thể cập nhật nickname",
            ["Profile.UpdateSuccess"] = "Cập nhật thông tin thành công",
            ["Profile.UpdateFailed"] = "Có lỗi xảy ra khi cập nhật thông tin: {0}",
            ["Payment.ConfigNotSetup"] = "Cấu hình thanh toán chưa được thiết lập",
            ["Payment.CannotCreateSession"] = "Không thể tạo phiên thanh toán",
            ["Payment.InvalidResponse"] = "Phản hồi từ cổng thanh toán không hợp lệ",
            ["Payment.PaymentFor"] = "Thanh toán cho user: {0}",
            ["Payment.TopupFor"] = "Nạp tiền vào tài khoản {0}",
            ["Payment.HistoryRetrievedSuccess"] = "Lấy lịch sử giao dịch thành công",
            ["Payment.HistoryRetrievedError"] = "Có lỗi xảy ra khi lấy lịch sử giao dịch: {0}",
            ["Payment.HistoryNotFound"] = "Không tìm thấy lịch sử giao dịch",
            ["Payment.ExpireSuccess"] = "Hủy phiên thanh toán thành công",
            ["Payment.ExpireError"] = "Không thể hủy phiên thanh toán: {0}",
            ["Payment.ExpireNotFound"] = "Không tìm thấy phiên thanh toán",
            ["Payment.ExpireInvalidSession"] = "ID phiên thanh toán không hợp lệ",
            ["Payment.ExpireAlreadyExpired"] = "Phiên thanh toán đã bị hủy hoặc đã thanh toán",
            
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
            
            // Password change messages
            ["Password.OldPasswordRequired"] = "Old password is required",
            ["Password.NewPasswordRequired"] = "New password is required",
            ["Password.ConfirmPasswordRequired"] = "Confirm password is required",
            ["Password.PasswordsDoNotMatch"] = "New password and confirm password do not match",
            ["Password.NewPasswordSameAsOld"] = "New password must be different from old password",
            ["Password.ChangeSuccess"] = "Password changed successfully",
            ["Password.OldPasswordIncorrect"] = "Old password is incorrect",
            ["Password.ChangeFailed"] = "An error occurred while changing password: {0}",
            ["Password.Required"] = "Password is required",
            ["Password.TooShort"] = "Password must be at least 6 characters",
            
            // Change Email messages
            ["ChangeEmail.AllFieldsRequired"] = "Please fill in all fields",
            ["ChangeEmail.InvalidEmailFormat"] = "Invalid email format",
            ["ChangeEmail.EmailsDoNotMatch"] = "New email and confirm email do not match",
            ["ChangeEmail.SameAsCurrentEmail"] = "New email must be different from current email",
            ["ChangeEmail.EmailAlreadyExists"] = "This email is already used by another account",
            ["ChangeEmail.NewEmailRequired"] = "Please provide new email",
            ["ChangeEmail.OtpCreated"] = "OTP created",
            ["ChangeEmail.OtpCreationFailed"] = "Failed to create OTP",
            ["ChangeEmail.OtpSentToCurrentEmail"] = "OTP code has been sent to your current email",
            ["ChangeEmail.OtpRequired"] = "Please enter OTP code",
            ["ChangeEmail.OtpVerified"] = "OTP verified successfully",
            ["ChangeEmail.OtpSentToNewEmail"] = "OTP code has been sent to your new email",
            ["ChangeEmail.EmailMismatch"] = "Email does not match",
            ["ChangeEmail.UpdateFailed"] = "Unable to update email",
            ["ChangeEmail.Success"] = "Email changed successfully! Please login again with your new email.",
            ["ChangeEmail.InvalidStep"] = "Invalid verification step",
            ["ChangeEmail.NoActiveRequest"] = "No active email change request found",
            ["ChangeEmail.OtpResentToCurrentEmail"] = "OTP resent to current email",
            ["ChangeEmail.OtpResentToNewEmail"] = "OTP resent to new email",
            
            // Change Email - Old Email OTP Template
            ["ChangeEmail.OldEmail.Subject"] = "Verify Email Change - Step 1",
            ["ChangeEmail.OldEmail.Title"] = "Verify Email Change",
            ["ChangeEmail.OldEmail.Greeting"] = "Hello {0}!",
            ["ChangeEmail.OldEmail.Intro"] = "You have requested to change your security email for your Gunny Game account. To confirm this is you, please use the OTP code below:",
            ["ChangeEmail.OldEmail.OtpLabel"] = "Your OTP code:",
            ["ChangeEmail.OldEmail.Validity"] = "This code is valid for 15 minutes",
            ["ChangeEmail.OldEmail.WarningTitle"] = "Security Warning",
            ["ChangeEmail.OldEmail.Warning"] = "If you did not request an email change, please ignore this email and change your password immediately to protect your account.",
            ["ChangeEmail.OldEmail.Ignore"] = "After verifying your old email, you will receive another OTP code at your new email to complete the process.",
            ["ChangeEmail.OldEmail.AutoMessage"] = "This is an automated email, please do not reply.",
            ["ChangeEmail.OldEmail.Copyright"] = "&copy; 2026 Gunny Game. All rights reserved.",
            
            // Change Email - New Email OTP Template
            ["ChangeEmail.NewEmail.Subject"] = "Verify New Email - Step 2",
            ["ChangeEmail.NewEmail.Title"] = "Verify New Email",
            ["ChangeEmail.NewEmail.Greeting"] = "Hello {0}!",
            ["ChangeEmail.NewEmail.Intro"] = "You have successfully verified your old email. Now, please verify this new email to complete the security email change process:",
            ["ChangeEmail.NewEmail.OtpLabel"] = "Your OTP code:",
            ["ChangeEmail.NewEmail.Validity"] = "This code is valid for 15 minutes",
            ["ChangeEmail.NewEmail.WarningTitle"] = "Important Note",
            ["ChangeEmail.NewEmail.Warning"] = "After verification, this email will become the official security email for your account. You will need to login again with your new email.",
            ["ChangeEmail.NewEmail.Ignore"] = "If you did not perform this action, please ignore this email.",
            ["ChangeEmail.NewEmail.AutoMessage"] = "This is an automated email, please do not reply.",
            ["ChangeEmail.NewEmail.Copyright"] = "&copy; 2026 Gunny Game. All rights reserved.",
            
            // Forgot Password Email Template
            ["ForgotPassword.EmailSubject"] = "Reset Password - Gunny Game",
            ["ForgotPassword.EmailTitle"] = "🔐 Reset Password",
            ["ForgotPassword.EmailGreeting"] = "Hello {0}!",
            ["ForgotPassword.EmailIntro"] = "You have requested to reset your password for your Gunny Game account.",
            ["ForgotPassword.EmailClickButton"] = "Click the button below to reset your password:",
            ["ForgotPassword.EmailButtonText"] = "Reset Password",
            ["ForgotPassword.EmailOrCopyLink"] = "Or copy this link to your browser:",
            ["ForgotPassword.EmailWarningTitle"] = "⚠️ Please note:",
            ["ForgotPassword.EmailWarning1"] = "This link will expire in <strong>1 hour</strong>",
            ["ForgotPassword.EmailWarning2"] = "Can only be used <strong>once</strong>",
            ["ForgotPassword.EmailWarning3"] = "Do not share this link with anyone",
            ["ForgotPassword.EmailIgnore"] = "If you did not request a password reset, please ignore this email. Your password will not be changed.",
            ["ForgotPassword.EmailAutoMessage"] = "This is an automated email, please do not reply.",
            ["ForgotPassword.EmailCopyright"] = "&copy; 2026 Gunny Game. All rights reserved.",
            ["ForgotPassword.EmailSent"] = "If the email exists, we have sent a password reset link to your email.",
            ["ForgotPassword.ProcessError"] = "Error processing request: {0}",
            ["User.EmailInvalid"] = "Invalid email",
            
            // Reset Password messages
            ["ResetPassword.TokenInvalid"] = "Invalid token",
            ["ResetPassword.TokenExpired"] = "Token is invalid or has expired",
            ["ResetPassword.TokenValid"] = "Token is valid",
            ["ResetPassword.VerifyTokenError"] = "Error verifying token: {0}",
            ["ResetPassword.Success"] = "Password reset successfully!",
            ["ResetPassword.Failed"] = "Unable to reset password. Please try again.",
            ["ResetPassword.Error"] = "Error resetting password: {0}",
            
            // Profile update messages
            ["Profile.UsernameRequired"] = "Username is required",
            ["Profile.NicknameRequired"] = "Nickname is required",
            ["Profile.InvalidEmailFormat"] = "Invalid email format",
            ["Profile.UsernameLengthInvalid"] = "Username must be between 5-100 characters",
            ["Profile.NicknameLengthInvalid"] = "Nickname must be between 3-20 characters",
            ["Profile.NicknameInvalidCharacters"] = "Nickname can only contain letters, numbers, spaces, underscores and hyphens",
            ["Profile.NoChangesDetected"] = "No changes detected",
            ["Profile.UsernameAlreadyExists"] = "Username '{0}' already exists",
            ["Profile.NicknameAlreadyExists"] = "Nickname '{0}' already exists",
            ["Profile.UpdateUsernameFailed"] = "Failed to update username",
            ["Profile.UpdateNicknameFailed"] = "Failed to update nickname",
            ["Profile.UpdateSuccess"] = "Profile updated successfully",
            ["Profile.UpdateFailed"] = "An error occurred while updating profile: {0}",
            ["Payment.InvalidSignature"] = "Invalid webhook signature",
            ["Payment.Unauthorized"] = "Unable to authenticate user",
            ["Payment.ConfigNotSetup"] = "Payment configuration not set up",
            ["Payment.CannotCreateSession"] = "Unable to create payment session",
            ["Payment.InvalidResponse"] = "Invalid response from payment gateway",
            ["Payment.PaymentFor"] = "Payment for user: {0}",
            ["Payment.TopupFor"] = "Top-up for account {0}",
            ["Payment.HistoryRetrievedSuccess"] = "Payment history retrieved successfully",
            ["Payment.HistoryRetrievedError"] = "An error occurred while retrieving payment history: {0}",
            ["Payment.HistoryNotFound"] = "Payment history not found",
            ["Payment.ExpireSuccess"] = "Checkout session expired successfully",
            ["Payment.ExpireError"] = "Cannot expire checkout session: {0}",
            ["Payment.ExpireNotFound"] = "Checkout session not found",
            ["Payment.ExpireInvalidSession"] = "Invalid checkout session ID",
            ["Payment.ExpireAlreadyExpired"] = "Checkout session is already expired or paid",
            
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
