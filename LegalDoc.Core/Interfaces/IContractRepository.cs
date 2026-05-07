using LegalDoc.Core.Models;
using LegalDoc.Core.Enums;

namespace LegalDoc.Core.Interfaces
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId);
        Task<IEnumerable<Contract>> GetByStatusAsync(ContractStatus status);
        Task<IEnumerable<Contract>> GetByUserIdAsync(int userId);
        Task<Contract> CreateAsync(Contract contract);
        Task<Contract> UpdateAsync(Contract contract);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}