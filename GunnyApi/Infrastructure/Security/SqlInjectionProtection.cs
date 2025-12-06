using System.Text.RegularExpressions;

namespace GunnyApi.Infrastructure.Security;

/// <summary>
/// Bảo vệ chống SQL Injection
/// </summary>
public static class SqlInjectionProtection
{
    private static readonly string[] DangerousPatterns = new[]
    {
        @"(\s|^)(exec|execute|sp_executesql)(\s|\()",
        @"(\s|^)(select|insert|update|delete|drop|create|alter|truncate)(\s|\()",
        @"(union\s+select|union\s+all\s+select)",
        @"(;|--|/\*|\*/|xp_|sp_)",
        @"('|""|;|=|>|<|--)",
        @"(script|javascript|vbscript|onload|onerror)",
        @"(char\(|nchar\(|varchar\(|nvarchar\(|cast\(|convert\()",
        @"(waitfor\s+delay|benchmark\(|sleep\()",
        @"(\bor\b|\band\b)\s*(1|true)\s*=\s*(1|true)",
        @"(\bor\b)\s+\d+\s*=\s*\d+",
        @"(information_schema|sysobjects|syscolumns|sys\.)"
    };

    private static readonly Regex CombinedPattern = new Regex(
        string.Join("|", DangerousPatterns),
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    /// <summary>
    /// Kiểm tra chuỗi có chứa pattern nguy hiểm không
    /// </summary>
    public static bool IsSuspicious(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return CombinedPattern.IsMatch(input);
    }

    /// <summary>
    /// Validate input và throw exception nếu phát hiện SQL injection
    /// </summary>
    public static void ValidateInput(string? input, string parameterName)
    {
        if (IsSuspicious(input))
        {
            throw new SecurityException(
                $"Phát hiện mẫu SQL nguy hiểm trong tham số '{parameterName}'. " +
                $"Giá trị đã bị chặn để bảo vệ hệ thống.");
        }
    }

    /// <summary>
    /// Validate nhiều inputs cùng lúc
    /// </summary>
    public static void ValidateInputs(params (string? value, string name)[] inputs)
    {
        foreach (var (value, name) in inputs)
        {
            ValidateInput(value, name);
        }
    }

    /// <summary>
    /// Làm sạch input bằng cách remove các ký tự nguy hiểm
    /// Chỉ nên dùng khi không thể validate và reject
    /// </summary>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove các ký tự nguy hiểm
        var sanitized = input
            .Replace("'", "''")  // Escape single quotes
            .Replace("--", "")    // Remove SQL comments
            .Replace(";", "")     // Remove statement separators
            .Replace("/*", "")    // Remove block comments
            .Replace("*/", "");

        return sanitized;
    }
}

public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
}
