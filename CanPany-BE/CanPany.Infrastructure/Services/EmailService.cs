using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CanPany.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly II18nService _i18nService;
    private readonly SendGridClient _client;

    public EmailService(
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger,
        II18nService i18nService)
    {
        _options = options.Value;
        _logger = logger;
        _i18nService = i18nService;
        
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("SendGrid API Key is missing. Email sending will fail.");
        }
        
        _client = new SendGridClient(_options.ApiKey);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = _i18nService.GetDisplayMessage("Email.Welcome.Subject");
        var bodyTemplate = _i18nService.GetDisplayMessage("Email.Welcome.Body");
        
        var body = EmailTemplates.GetWelcomeEmail(bodyTemplate, userName);

        await SendEmailAsync(toEmail, subject, body);
        _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetCode)
    {
        var subject = _i18nService.GetDisplayMessage("Email.PasswordReset.Subject");
        var bodyTemplate = _i18nService.GetDisplayMessage("Email.PasswordReset.Body");
        var expirationMinutes = "15"; // Hardcoded or config
        
        var body = EmailTemplates.GetPasswordResetEmail(bodyTemplate, userName, resetCode, expirationMinutes);

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendApplicationStatusEmailAsync(string toEmail, string candidateName, string jobTitle, string status)
    {
        string subjectKey;
        string bodyKey;

        if (status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
        {
            subjectKey = "Email.ApplicationStatus.Accepted.Subject";
            bodyKey = "Email.ApplicationStatus.Accepted.Body";
            
            var subjectTemplate = _i18nService.GetDisplayMessage(subjectKey);
            var subject = string.Format(subjectTemplate, jobTitle);
            
            var bodyTemplate = _i18nService.GetDisplayMessage(bodyKey);
            var body = EmailTemplates.GetApplicationAcceptedEmail(bodyTemplate, candidateName, jobTitle);
            
            await SendEmailAsync(toEmail, subject, body);
        }
        else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            subjectKey = "Email.ApplicationStatus.Rejected.Subject";
            bodyKey = "Email.ApplicationStatus.Rejected.Body";
            
            var subjectTemplate = _i18nService.GetDisplayMessage(subjectKey);
            var subject = string.Format(subjectTemplate, jobTitle);

            var bodyTemplate = _i18nService.GetDisplayMessage(bodyKey);
            var body = EmailTemplates.GetApplicationRejectedEmail(bodyTemplate, candidateName, jobTitle);

            await SendEmailAsync(toEmail, subject, body);
        }
        else
        {
            _logger.LogWarning("Email notification skipped for status: {Status}", status);
            return;
        }

        _logger.LogInformation("Application status email ({Status}) sent to {Email}", status, toEmail);
    }

    public async Task SendJobMatchEmailAsync(string toEmail, string candidateName, string jobTitle, string jobId, string companyName, string location, string budgetInfo)
    {
        var subject = _i18nService.GetDisplayMessage("Email.JobMatch.Subject");
        var bodyTemplate = _i18nService.GetDisplayMessage("Email.JobMatch.Body");
        
        var body = EmailTemplates.GetJobMatchEmail(bodyTemplate, candidateName, jobTitle, companyName, location, budgetInfo);

        await SendEmailAsync(toEmail, subject, body);
        _logger.LogInformation("Job match email sent to {Email} for job {JobTitle}", toEmail, jobTitle);
    }

    public async Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, string paymentId, long amount, string currency, string status, string purpose, DateTime paidAt)
    {
        string subjectKey = status == "Paid" 
            ? "Email.PaymentConfirmation.Success.Subject" 
            : "Email.PaymentConfirmation.Failed.Subject";
        string bodyKey = status == "Paid" 
            ? "Email.PaymentConfirmation.Success.Body" 
            : "Email.PaymentConfirmation.Failed.Body";

        var subject = _i18nService.GetDisplayMessage(subjectKey);
        var bodyTemplate = _i18nService.GetDisplayMessage(bodyKey);
        
        var body = EmailTemplates.GetPaymentConfirmationEmail(bodyTemplate, userName, paymentId, amount, currency, status, purpose, paidAt);

        await SendEmailAsync(toEmail, subject, body);
        _logger.LogInformation("Payment confirmation email ({Status}) sent to {Email}", status, toEmail);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string plainTextContent)
    {
        try
        {
            var from = new EmailAddress(_options.SenderEmail, _options.SenderName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, null);

            var response = await _client.SendEmailAsync(msg);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email} with subject '{Subject}'. Status: {Status}", 
                    toEmail, subject, response.StatusCode);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Body: {Body}", 
                    toEmail, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email to {Email}", toEmail);
            throw; 
        }
    }
}
