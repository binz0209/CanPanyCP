using CanPany.Worker.Models;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Handlers.Samples;

/// <summary>
/// Sample handler for AI matching jobs
/// Handles: Job.AIMatching.*, Job.AI.CalculateMatch
/// </summary>
public class AIMatchingJobHandler : BaseJobHandler
{
    public AIMatchingJobHandler(ILogger<AIMatchingJobHandler> logger) : base(logger)
    {
    }

    public override string[] SupportedI18nKeys => new[]
    {
        "Job.AIMatching.*",
        "Job.AI.CalculateMatch"
    };

    public override async Task<JobResult> ExecuteAsync(JobMessage job, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "[AI_MATCHING_START] JobId: {JobId} | I18nKey: {I18nKey}",
            job.JobId,
            job.I18nKey
        );

        try
        {
            var payload = DeserializePayload<AIMatchingPayload>(job.Payload);
            
            if (payload == null)
                return JobResult.FailureResult("Invalid payload", "INVALID_PAYLOAD");

            // Simulate AI processing with progress updates
            await ReportProgressAsync(job.JobId, 10, "Loading candidate profile...", null, cancellationToken);
            await Task.Delay(Random.Shared.Next(200, 500), cancellationToken);

            await ReportProgressAsync(job.JobId, 30, "Loading job requirements...", null, cancellationToken);
            await Task.Delay(Random.Shared.Next(200, 500), cancellationToken);

            await ReportProgressAsync(job.JobId, 50, "Analyzing skills match...", null, cancellationToken);
            await Task.Delay(Random.Shared.Next(300, 700), cancellationToken);

            await ReportProgressAsync(job.JobId, 70, "Calculating experience compatibility...", null, cancellationToken);
            await Task.Delay(Random.Shared.Next(300, 700), cancellationToken);

            await ReportProgressAsync(job.JobId, 90, "Generating final match score...", null, cancellationToken);
            await Task.Delay(Random.Shared.Next(200, 500), cancellationToken);

            var matchScore = Random.Shared.NextDouble() * 100;

            await ReportProgressAsync(job.JobId, 100, "AI matching completed!", new Dictionary<string, object>
            {
                ["MatchScore"] = matchScore
            }, cancellationToken);

            Logger.LogInformation(
                "[AI_MATCHING_COMPLETE] CandidateId: {CandidateId} | JobId: {JobId} | Score: {Score:F2}",
                payload.CandidateId,
                payload.JobId,
                matchScore
            );

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["CandidateId"] = payload.CandidateId,
                ["JobId"] = payload.JobId,
                ["MatchScore"] = matchScore
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[AI_MATCHING_FAILED] JobId: {JobId}", job.JobId);
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }
}

public record AIMatchingPayload(
    string CandidateId,
    string JobId,
    string? CVId = null
);
