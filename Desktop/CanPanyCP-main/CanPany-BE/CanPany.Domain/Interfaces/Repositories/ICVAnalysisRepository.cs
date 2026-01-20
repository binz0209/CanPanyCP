using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for CVAnalysis entity
/// </summary>
public interface ICVAnalysisRepository
{
    Task<CVAnalysis?> GetByIdAsync(string id);
    Task<CVAnalysis?> GetByCVIdAsync(string cvId);
    Task<IEnumerable<CVAnalysis>> GetByCandidateIdAsync(string candidateId);
    Task<CVAnalysis> AddAsync(CVAnalysis cvAnalysis);
    Task UpdateAsync(CVAnalysis cvAnalysis);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

