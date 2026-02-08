using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Conversation entity
/// </summary>
public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(string id);
    Task<Conversation?> GetByParticipantsAsync(string userId1, string userId2, string? jobId = null);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
    Task<Conversation> AddAsync(Conversation conversation);
    Task UpdateAsync(Conversation conversation);
    Task DeleteAsync(string id);
}
