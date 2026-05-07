using LegalDoc.Core.Models;
using LegalDoc.Core.Enums;

namespace LegalDoc.Core.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(int id);
        Task<IEnumerable<Client>> GetAllAsync();
        Task<IEnumerable<Client>> GetByTypeAsync(ClientType type);
        Task<Client?> GetByEmailAsync(string email);
        Task<Client> CreateAsync(Client client);
        Task<Client> UpdateAsync(Client client);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
    }
}