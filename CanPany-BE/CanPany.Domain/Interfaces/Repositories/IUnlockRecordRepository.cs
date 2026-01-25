using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

public interface IUnlockRecordRepository
{
    Task<UnlockRecord> AddAsync(UnlockRecord unlockRecord);
    Task<bool> HasUnlockedAsync(string companyId, string candidateId);
    Task<IEnumerable<UnlockRecord>> GetByCompanyIdAsync(string companyId, int page, int pageSize);
    Task<IEnumerable<UnlockRecord>> GetByCandidateIdAsync(string candidateId);
}
