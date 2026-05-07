using LegalDoc.Core.Models;

namespace LegalDoc.Core.Interfaces
{
    public interface ITemplateRepository
    {
        Task<Template?> GetByIdAsync(int id);
        Task<IEnumerable<Template>> GetAllAsync();
        Task<IEnumerable<Template>> GetActivesAsync();
        Task<IEnumerable<Template>> GetByCategoryAsync(string category);
        Task<Template> CreateAsync(Template template);
        Task<Template> UpdateAsync(Template template);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}