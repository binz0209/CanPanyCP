using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Contracts controller — CRUD for formal contracts created from accepted applications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IContractService contractService,
        ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/contracts/my — Get contracts for the authenticated user (candidate or company).
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyContracts([FromQuery] string role = "candidate")
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            IEnumerable<Contract> contracts;
            if (role.Equals("company", StringComparison.OrdinalIgnoreCase))
            {
                contracts = await _contractService.GetByCompanyIdAsync(userId);
            }
            else
            {
                contracts = await _contractService.GetByCandidateIdAsync(userId);
            }

            return Ok(ApiResponse<IEnumerable<Contract>>.CreateSuccess(contracts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contracts");
            return StatusCode(500, ApiResponse.CreateError("Failed to get contracts", "GetContractsFailed"));
        }
    }

    /// <summary>
    /// GET /api/contracts/{id} — Get contract by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetContract(string id)
    {
        try
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null)
                return NotFound(ApiResponse.CreateError("Contract not found", "NotFound"));

            return Ok(ApiResponse<Contract>.CreateSuccess(contract));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract: {Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to get contract", "GetContractFailed"));
        }
    }

    /// <summary>
    /// POST /api/contracts — Create a contract from an accepted application.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var contract = await _contractService.CreateFromApplicationAsync(
                request.ApplicationId
                , userId,
                request.AgreedAmount,
                request.StartDate,
                request.EndDate);

            return Ok(ApiResponse<Contract>.CreateSuccess(contract, "Contract created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message, "InvalidOperation"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract");
            return StatusCode(500, ApiResponse.CreateError("Failed to create contract", "CreateContractFailed"));
        }
    }

    /// <summary>
    /// PUT /api/contracts/{id}/status — Update contract status.
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateContractStatus(string id, [FromBody] UpdateContractStatusRequest request)
    {
        try
        {
            await _contractService.UpdateStatusAsync(id, request.Status, request.CancellationReason);
            return Ok(ApiResponse.CreateSuccess($"Contract status updated to {request.Status}"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message, "InvalidStatus"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse.CreateError(ex.Message, "NotFound"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract status: {Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to update contract status", "UpdateStatusFailed"));
        }
    }
}

public record CreateContractRequest(
    string ApplicationId,
    decimal AgreedAmount,
    DateTime? StartDate,
    DateTime? EndDate);

public record UpdateContractStatusRequest(
    string Status,
    string? CancellationReason);
