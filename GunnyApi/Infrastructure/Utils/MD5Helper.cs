using System.Security.Cryptography;
using System.Text;

namespace GunnyApi.Infrastructure.Utils;

/// <summary>
/// Helper class để mã hóa MD5
/// </summary>
public static class MD5Helper
{
    /// <summary>
    /// Chuyển string sang MD5 hash
    /// </summary>
    /// <param name="input">Chuỗi cần mã hóa</param>
    /// <returns>Chuỗi MD5 hash (32 ký tự hex)</returns>
    public static string ToMD5(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input không được rỗng", nameof(input));
        }

        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);

        // Convert byte array sang hex string
        var sb = new StringBuilder();
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Chuyển string sang MD5 hash in uppercase
    /// </summary>
    /// <param name="input">Chuỗi cần mã hóa</param>
    /// <returns>Chuỗi MD5 hash uppercase (32 ký tự hex)</returns>
    public static string ToMD5Upper(string input)
    {
        return ToMD5(input).ToUpper();
    }
}
