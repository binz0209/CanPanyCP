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

    public static string GetApplicationAcceptedEmail(string template, string candidateName, string jobTitle)
    {
        var sb = new StringBuilder();
        sb.AppendLine("********************************************");
        sb.AppendLine("*                                          *");
        sb.AppendLine("*            CONGRATULATIONS!              *");
        sb.AppendLine("*                                          *");
        sb.AppendLine("********************************************");
        sb.AppendLine();
        sb.AppendLine(string.Format(template, candidateName, jobTitle));
        sb.AppendLine();
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine("NEXT STEPS:");
        sb.AppendLine("Please check the 'My Applications' section");
        sb.AppendLine("in the CanPany app for further instructions.");
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine();
        sb.AppendLine("CanPany Recruitment Team");
        sb.AppendLine("https://canpany.com");
        sb.AppendLine("============================================");
        return sb.ToString();
    }

    public static string GetApplicationRejectedEmail(string template, string candidateName, string jobTitle)
    {
        var sb = new StringBuilder();
        sb.AppendLine("============================================");
        sb.AppendLine("            APPLICATION UPDATE");
        sb.AppendLine("============================================");
        sb.AppendLine();
        sb.AppendLine(string.Format(template, candidateName, jobTitle));
        sb.AppendLine();
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine("CanPany Recruitment Team");
        sb.AppendLine("https://canpany.com");
        sb.AppendLine("============================================");
        return sb.ToString();
    }
}
