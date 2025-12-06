using System.Security.Cryptography;
using System.Text;

namespace GunnyApi.Infrastructure.Utils;

/// <summary>
/// Helper class cho mã hóa/giải mã
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// Mã hóa dữ liệu bằng Triple DES
    /// </summary>
    public static string TripleDesEncrypt(string key, string plainText, ref string iv)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            byte[] keyBytes = HexStringToByteArray(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var des = TripleDES.Create();
            des.Key = keyBytes;
            des.IV = ivBytes;
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;

            using var encryptor = des.CreateEncryptor();
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Giải mã dữ liệu bằng Triple DES
    /// </summary>
    public static string TripleDesDecrypt(string key, string cipherText, string iv)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            byte[] keyBytes = HexStringToByteArray(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using var des = TripleDES.Create();
            des.Key = keyBytes;
            des.IV = ivBytes;
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;

            using var decryptor = des.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        int length = hex.Length / 2;
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }
}
