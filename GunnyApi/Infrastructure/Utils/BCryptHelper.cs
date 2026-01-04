namespace GunnyApi.Infrastructure.Utils
{
    public static class BCryptHelper
    {
        /// <summary>
        /// Băm mật khẩu bằng thuật toán BCrypt.
        /// </summary>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Xác thực mật khẩu thô với một chuỗi hash có sẵn.
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
