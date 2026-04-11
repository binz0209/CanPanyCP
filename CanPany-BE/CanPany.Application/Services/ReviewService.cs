using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Review management service — allows participants to rate each other after contract completion.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IContractRepository _contractRepo;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IReviewRepository reviewRepo,
        IContractRepository contractRepo,
        ILogger<ReviewService> logger)
    {
        _reviewRepo = reviewRepo;
        _contractRepo = contractRepo;
        _logger = logger;
    }

    public async Task<Review?> GetByIdAsync(string id)
    {
        return await _reviewRepo.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Review>> GetByContractIdAsync(string contractId)
    {
        return await _reviewRepo.GetByContractIdAsync(contractId);
    }

    public async Task<IEnumerable<Review>> GetByRevieweeIdAsync(string userId)
    {
        return await _reviewRepo.GetByRevieweeIdAsync(userId);
    }

    public async Task<Review> CreateAsync(string contractId, string reviewerId, string revieweeId, int rating, string? comment)
    {
        // Validate rating
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        // Verify contract exists and is completed
        var contract = await _contractRepo.GetByIdAsync(contractId)
            ?? throw new InvalidOperationException($"Contract {contractId} not found");

        if (contract.Status != "Completed")
        {
            throw new InvalidOperationException("Reviews can only be submitted for completed contracts");
        }

        // Verify reviewer is a participant
        if (reviewerId != contract.CompanyId && reviewerId != contract.CandidateId)
        {
            throw new UnauthorizedAccessException("Only contract participants can submit reviews");
        }

        // Prevent duplicate reviews
        var existingReviews = await _reviewRepo.GetByContractIdAsync(contractId);
        if (existingReviews.Any(r => r.ReviewerId == reviewerId))
        {
            throw new InvalidOperationException("You have already submitted a review for this contract");
        }

        var review = new Review
        {
            ContractId = contractId,
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _reviewRepo.AddAsync(review);
        _logger.LogInformation("Review created: {ReviewId} for contract {ContractId} (rating: {Rating})",
            created.Id, contractId, rating);

        return created;
    }
}
