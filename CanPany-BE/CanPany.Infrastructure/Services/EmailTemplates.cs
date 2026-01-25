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

    public static string GetJobMatchEmail(string template, string candidateName, string jobTitle, string companyName, string location, string budgetInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("********************************************");
        sb.AppendLine("*                                          *");
        sb.AppendLine("*          NEW JOB MATCH FOUND!            *");
        sb.AppendLine("*                                          *");
        sb.AppendLine("********************************************");
        sb.AppendLine();
        sb.AppendLine(string.Format(template, candidateName, jobTitle, companyName, location, budgetInfo));
        sb.AppendLine();
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine("QUICK ACTIONS:");
        sb.AppendLine("• View job details in the CanPany app");
        sb.AppendLine("• Apply now to increase your chances");
        sb.AppendLine("• Save for later review");
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine();
        sb.AppendLine("CanPany Job Alerts");
        sb.AppendLine("https://canpany.com");
        sb.AppendLine("============================================");
        return sb.ToString();
    }

    public static string GetPaymentConfirmationEmail(string template, string userName, string paymentId, long amount, string currency, string status, string purpose, DateTime paidAt)
    {
        var amountFormatted = FormatAmount(amount, currency);
        var statusIcon = status == "Paid" ? "✓" : "✗";
        var statusText = status == "Paid" ? "SUCCESSFUL" : "FAILED";
        
        var sb = new StringBuilder();
        sb.AppendLine("============================================");
        sb.AppendLine($"      PAYMENT {statusText} {statusIcon}");
        sb.AppendLine("============================================");
        sb.AppendLine();
        sb.AppendLine(string.Format(template, userName, amountFormatted, purpose, statusText));
        sb.AppendLine();
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine("PAYMENT DETAILS:");
        sb.AppendLine($"Payment ID: {paymentId}");
        sb.AppendLine($"Amount: {amountFormatted}");
        sb.AppendLine($"Purpose: {purpose}");
        sb.AppendLine($"Status: {statusText}");
        sb.AppendLine($"Date: {paidAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine();
        
        if (status == "Paid")
        {
            sb.AppendLine("Thank you for your payment!");
            if (purpose == "TopUp")
            {
                sb.AppendLine("Your wallet has been updated.");
            }
            else if (purpose == "Premium")
            {
                sb.AppendLine("Your premium features are now active.");
            }
        }
        else
        {
            sb.AppendLine("If you have any questions, please contact support.");
        }
        
        sb.AppendLine();
        sb.AppendLine("CanPany Payment Team");
        sb.AppendLine("Email: payment@canpany.com");
        sb.AppendLine("https://canpany.com");
        sb.AppendLine("============================================");
        return sb.ToString();
    }

    private static string FormatAmount(long amount, string currency)
    {
        // Amount is in minor units (e.g., cents for USD, đồng for VND)
        if (currency == "VND")
        {
            return $"{amount:N0} {currency}";
        }
        else
        {
            var majorAmount = amount / 100.0m;
            return $"{majorAmount:N2} {currency}";
        }
    }
}

