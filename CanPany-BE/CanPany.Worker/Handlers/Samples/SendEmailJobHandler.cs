using CanPany.Worker.Models;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Handlers.Samples;

/// <summary>
/// Sample handler for email jobs
/// Handles: Job.SendEmail.*, Job.Notification.Email.*
/// </summary>
public class SendEmailJobHandler : BaseJobHandler
{
    public SendEmailJobHandler(ILogger<SendEmailJobHandler> logger) : base(logger)
    {
    }

    public override string[] SupportedI18nKeys => new[]
    {
        "Job.SendEmail.*",
        "Job.Notification.Email.*"
    };

    public override async Task<JobResult> ExecuteAsync(JobMessage job, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "[EMAIL_JOB_START] JobId: {JobId} | I18nKey: {I18nKey}",
            job.JobId,
            job.I18nKey
        );

        try
        {
            var payload = DeserializePayload<EmailPayload>(job.Payload);
            
            if (payload == null)
                return JobResult.FailureResult("Invalid payload", "INVALID_PAYLOAD");

            // Simulate email sending
            await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);

            Logger.LogInformation(
                "[EMAIL_SENT] To: {Email} | Subject: {Subject}",
                payload.Email,
                payload.Subject
            );

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["EmailSentTo"] = payload.Email,
                ["Subject"] = payload.Subject
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[EMAIL_JOB_FAILED] JobId: {JobId}", job.JobId);
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }
}

public record EmailPayload(
    string Email,
    string Subject,
    string Body,
    string? TemplateKey = null
);
