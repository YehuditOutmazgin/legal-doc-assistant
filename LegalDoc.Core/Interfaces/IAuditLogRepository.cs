using LegalDoc.Core.Models;

namespace LegalDoc.Core.Interfaces
{
    public interface IAuditLogRepository
    {
        Task LogAsync(int contractId, int userId, string action, string? details = null);
        Task<IEnumerable<AuditLog>> GetByContractIdAsync(int contractId);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50);
    }
}