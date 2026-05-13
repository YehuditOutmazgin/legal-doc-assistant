using LegalDoc.Core.DTOs;
using LegalDoc.Core.Enums;

namespace LegalDoc.Core.Interfaces
{
    public interface IClientService
    {
        Task<ClientDto> CreateAsync(CreateClientDto createDto);
        Task<ClientDto> UpdateAsync(int id, UpdateClientDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<ClientDto> GetByIdAsync(int id);
        Task<IEnumerable<ClientDto>> GetAllAsync();
        Task<IEnumerable<ClientDto>> GetByTypeAsync(ClientType type);
        Task<ClientDto> GetByEmailAsync(string email);
    }
}
