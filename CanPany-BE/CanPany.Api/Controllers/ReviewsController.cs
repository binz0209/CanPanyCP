using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Reviews controller — Post-contract review/rating system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        IReviewService reviewService,
        ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/reviews/contract/{contractId} — Get reviews for a contract.
    /// </summary>
    [HttpGet("contract/{contractId}")]
    public async Task<IActionResult> GetByContract(string contractId)
    {
        try
        {
            var reviews = await _reviewService.GetByContractIdAsync(contractId);
            return Ok(ApiResponse<IEnumerable<Review>>.CreateSuccess(reviews));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for contract: {ContractId}", contractId);
            return StatusCode(500, ApiResponse.CreateError("Failed to get reviews", "GetReviewsFailed"));
        }
    }

    /// <summary>
    /// GET /api/reviews/user/{userId} — Get reviews received by a user.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
    {
        try
        {
            var reviews = await _reviewService.GetByRevieweeIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<Review>>.CreateSuccess(reviews));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for user: {UserId}", userId);
            return StatusCode(500, ApiResponse.CreateError("Failed to get reviews", "GetReviewsFailed"));
        }
    }

    /// <summary>
    /// POST /api/reviews — Submit a review for a completed contract.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var review = await _reviewService.CreateAsync(
                request.ContractId,
                userId,
                request.RevieweeId,
                request.Rating,
                request.Comment);

            return Ok(ApiResponse<Review>.CreateSuccess(review, "Review submitted successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message, "InvalidRating"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message, "InvalidOperation"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return StatusCode(500, ApiResponse.CreateError("Failed to create review", "CreateReviewFailed"));
        }
    }
}

public record CreateReviewRequest(
    string ContractId,
    string RevieweeId,
    int Rating,
    string? Comment);
