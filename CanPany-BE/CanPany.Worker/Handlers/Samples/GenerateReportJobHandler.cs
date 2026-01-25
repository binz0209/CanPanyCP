using CanPany.Worker.Models;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Handlers.Samples;

/// <summary>
/// Sample handler for report generation jobs
/// Handles: Job.GenerateReport.*
/// </summary>
public class GenerateReportJobHandler : BaseJobHandler
{
    public GenerateReportJobHandler(ILogger<GenerateReportJobHandler> logger) : base(logger)
    {
    }

    public override string[] SupportedI18nKeys => new[]
    {
        "Job.GenerateReport.*",
        "Job.Export.Report.*"
    };

    public override async Task<JobResult> ExecuteAsync(JobMessage job, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "[REPORT_GEN_START] JobId: {JobId} | I18nKey: {I18nKey}",
            job.JobId,
            job.I18nKey
        );

        try
        {
            var payload = DeserializePayload<ReportPayload>(job.Payload);
            
            if (payload == null)
                return JobResult.FailureResult("Invalid payload", "INVALID_PAYLOAD");

            // Simulate multi-step report generation with progress tracking
            var totalSteps = 5;

            // Step 1: Fetching data
            await ReportStepsAsync(job.JobId, 0, totalSteps, "Fetching data from database...", cancellationToken);
            await Task.Delay(Random.Shared.Next(500, 1000), cancellationToken);

            // Step 2: Processing data
            await ReportStepsAsync(job.JobId, 1, totalSteps, "Processing data...", cancellationToken);
            await Task.Delay(Random.Shared.Next(800, 1500), cancellationToken);

            // Step 3: Generating charts
            await ReportStepsAsync(job.JobId, 2, totalSteps, "Generating charts and visualizations...", cancellationToken);
            await Task.Delay(Random.Shared.Next(600, 1200), cancellationToken);

            // Step 4: Creating PDF
            await ReportStepsAsync(job.JobId, 3, totalSteps, "Creating PDF document...", cancellationToken);
            await Task.Delay(Random.Shared.Next(700, 1300), cancellationToken);

            // Step 5: Uploading to storage
            await ReportStepsAsync(job.JobId, 4, totalSteps, "Uploading to cloud storage...", cancellationToken);
            await Task.Delay(Random.Shared.Next(500, 1000), cancellationToken);

            // Final step
            await ReportStepsAsync(job.JobId, 5, totalSteps, "Report generation completed!", cancellationToken);

            var reportUrl = $"https://cdn.canpany.com/reports/{Guid.NewGuid()}.pdf";

            Logger.LogInformation(
                "[REPORT_GENERATED] Type: {ReportType} | URL: {ReportUrl}",
                payload.ReportType,
                reportUrl
            );

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["ReportType"] = payload.ReportType,
                ["ReportUrl"] = reportUrl,
                ["GeneratedAt"] = DateTime.UtcNow,
                ["Steps"] = totalSteps
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[REPORT_GEN_FAILED] JobId: {JobId}", job.JobId);
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }
}

public record ReportPayload(
    string ReportType,
    DateTime StartDate,
    DateTime EndDate,
    string? UserId = null
);
