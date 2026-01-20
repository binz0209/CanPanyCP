namespace CanPany.Infrastructure.Services;

using System.Text;

public static class EmailTemplates
{
    private static string CreateLayout(string title, string content)
    {
        var sb = new StringBuilder();
        sb.AppendLine("============================================");
        sb.AppendLine($"   {title.ToUpper()}");
        sb.AppendLine("============================================");
        sb.AppendLine();
        sb.AppendLine(content);
        sb.AppendLine();
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine("CanPany Recruitment Platform");
        sb.AppendLine("Email: support@canpany.com");
        sb.AppendLine("Website: https://canpany.com");
        sb.AppendLine("============================================");
        return sb.ToString();
    }

    public static string GetWelcomeEmail(string template, string userName)
    {
        var body = string.Format(template, userName);
        return CreateLayout("Welcome", body);
    }

    public static string GetPasswordResetEmail(string template, string userName, string resetCode, string expirationMinutes)
    {
        var body = string.Format(template, userName, resetCode, expirationMinutes);
        return CreateLayout("Password Reset", body);
    }
}
