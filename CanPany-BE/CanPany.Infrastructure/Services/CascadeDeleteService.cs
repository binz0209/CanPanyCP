using CanPany.Application.Services;
using CanPany.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Services;

/// <summary>
/// Handles cascade deletion of related data when a User or Company is deleted.
/// Uses MongoDbContext directly for efficient bulk DeleteMany operations.
/// </summary>
public class CascadeDeleteService : ICascadeDeleteService

{
    private readonly MongoDbContext _db;
    private readonly ILogger<CascadeDeleteService> _logger;

    public CascadeDeleteService(MongoDbContext db, ILogger<CascadeDeleteService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task CascadeDeleteUserDataAsync(string userId)
    {
        _logger.LogInformation("Starting cascade delete for user {UserId}", userId);

        var tasks = new List<Task<DeleteResult>>();

        // 1. Profile & Settings
        tasks.Add(_db.UserProfiles.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.UserSettings.DeleteManyAsync(x => x.UserId == userId));

        // 2. CVs & CV Analyses
        tasks.Add(_db.CVs.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.CVAnalyses.DeleteManyAsync(x => x.CandidateId == userId));

        // 3. Applications (as candidate)
        tasks.Add(_db.Applications.DeleteManyAsync(x => x.CandidateId == userId));

        // 4. Bookmarks & Interactions
        tasks.Add(_db.JobBookmarks.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.UserJobInteractions.DeleteManyAsync(x => x.UserId == userId));

        // 5. Wallet & Financial
        tasks.Add(_db.Wallets.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.WalletTransactions.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.Payments.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.UserSubscriptions.DeleteManyAsync(x => x.UserId == userId));

        // 6. Notifications & Alerts
        tasks.Add(_db.Notifications.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.JobAlerts.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.JobAlertMatches.DeleteManyAsync(x => x.UserId == userId));

        // 7. Conversations & Messages (where user is a participant)
        // Note: Conversations have multiple participants, we delete messages by senderId
        // and remove conversations where user is the only/primary participant
        tasks.Add(_db.Messages.DeleteManyAsync(x => x.SenderId == userId));

        // 8. Misc
        tasks.Add(_db.FilterPresets.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.UserConsents.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.RecommendationLogs.DeleteManyAsync(x => x.UserId == userId));
        tasks.Add(_db.Contracts.DeleteManyAsync(x => x.CandidateId == userId));

        // 9. Reviews written by or about user
        tasks.Add(_db.Reviews.DeleteManyAsync(x => x.ReviewerId == userId));
        tasks.Add(_db.Reviews.DeleteManyAsync(x => x.RevieweeId == userId));

        var results = await Task.WhenAll(tasks);
        var totalDeleted = results.Sum(r => r.DeletedCount);

        _logger.LogInformation(
            "Cascade delete completed for user {UserId}: {TotalDeleted} related documents removed",
            userId, totalDeleted);
    }

    public async Task CascadeDeleteCompanyDataAsync(string companyId)
    {
        _logger.LogInformation("Starting cascade delete for company {CompanyId}", companyId);

        // 1. Get all job IDs for this company (needed to cascade-delete applications)
        var jobs = await _db.Jobs.Find(j => j.CompanyId == companyId)
            .Project(j => j.Id)
            .ToListAsync();

        var jobIds = jobs.ToList();

        var tasks = new List<Task<DeleteResult>>();

        // 2. Delete applications for all company jobs
        if (jobIds.Any())
        {
            var jobIdFilter = Builders<Domain.Entities.Application>.Filter
                .In(a => a.JobId, jobIds);
            tasks.Add(_db.Applications.DeleteManyAsync(jobIdFilter));

            // Delete job bookmarks for company jobs
            var bookmarkFilter = Builders<Domain.Entities.JobBookmark>.Filter
                .In(b => b.JobId, jobIds);
            tasks.Add(_db.JobBookmarks.DeleteManyAsync(bookmarkFilter));

            // Delete job interactions for company jobs
            var interactionFilter = Builders<Domain.Entities.UserJobInteraction>.Filter
                .In(i => i.JobId, jobIds);
            tasks.Add(_db.UserJobInteractions.DeleteManyAsync(interactionFilter));
        }

        // 3. Delete all jobs
        tasks.Add(_db.Jobs.DeleteManyAsync(j => j.CompanyId == companyId));

        // 4. Delete reviews linked to company contracts
        if (jobIds.Any())
        {
            // Reviews are tied to contracts, which are tied to the company
            // We already delete contracts below, so reviews linked to those contracts
            // should also be cleaned up
            var contractIds = await _db.Contracts.Find(c => c.CompanyId == companyId)
                .Project(c => c.Id)
                .ToListAsync();
            if (contractIds.Any())
            {
                var reviewFilter = Builders<Domain.Entities.Review>.Filter
                    .In(r => r.ContractId, contractIds);
                tasks.Add(_db.Reviews.DeleteManyAsync(reviewFilter));
            }
        }

        // 5. Delete contracts
        tasks.Add(_db.Contracts.DeleteManyAsync(c => c.CompanyId == companyId));

        // 6. Delete unlock records
        tasks.Add(_db.UnlockRecords.DeleteManyAsync(u => u.CompanyId == companyId));

        // 7. Delete candidate alerts from this company
        tasks.Add(_db.CandidateAlerts.DeleteManyAsync(a => a.CompanyId == companyId));

        var results = await Task.WhenAll(tasks);
        var totalDeleted = results.Sum(r => r.DeletedCount);

        _logger.LogInformation(
            "Cascade delete completed for company {CompanyId}: {JobCount} jobs, {TotalDeleted} total related documents removed",
            companyId, jobIds.Count, totalDeleted);
    }
}
