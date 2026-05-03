namespace CanPany.Application.Services;

/// <summary>
/// Handles cascade deletion of related data when a User or Company is deleted.
/// </summary>
public interface ICascadeDeleteService
{
    /// <summary>
    /// Delete all data owned by a user (Candidate): profile, CVs, applications, 
    /// bookmarks, wallet, notifications, conversations, etc.
    /// </summary>
    Task CascadeDeleteUserDataAsync(string userId);

    /// <summary>
    /// Delete all data owned by a company: jobs (and their applications), 
    /// reviews, conversations, contracts, etc.
    /// </summary>
    Task CascadeDeleteCompanyDataAsync(string companyId);
}
