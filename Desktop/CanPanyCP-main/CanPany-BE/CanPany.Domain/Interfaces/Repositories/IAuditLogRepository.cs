using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for AuditLog entity
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(string id);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int limit = 100);
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType, string? entityId = null, int limit = 100);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000);
    Task<AuditLog> AddAsync(AuditLog auditLog);
    Task DeleteAsync(string id);
}


