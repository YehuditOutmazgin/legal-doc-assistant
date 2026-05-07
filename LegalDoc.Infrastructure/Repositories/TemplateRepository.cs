using LegalDoc.Core.Models;
using LegalDoc.Core.Interfaces;
using LegalDoc.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;

namespace LegalDoc.Infrastructure.Repositories
{
    public class TemplateRepository : BaseRepository, ITemplateRepository
    {
        public TemplateRepository(string connectionString, ILogger<TemplateRepository> logger)
            : base(connectionString, logger)
        {
        }

        public async Task<Template?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT ID, NAME, DESCRIPTION, CONTENT, CATEGORY, CREATED_BY_USER_ID, 
                       CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM TEMPLATES
                WHERE ID = :id";

            return await ExecuteReaderSingleAsync(query, MapTemplate, CreateParameter(":id", id));
        }

        public async Task<IEnumerable<Template>> GetAllAsync()
        {
            const string query = @"
                SELECT ID, NAME, DESCRIPTION, CONTENT, CATEGORY, CREATED_BY_USER_ID, 
                       CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM TEMPLATES
                ORDER BY NAME";

            return await ExecuteReaderAsync(query, MapTemplate);
        }

        public async Task<IEnumerable<Template>> GetActivesAsync()
        {
            const string query = @"
                SELECT ID, NAME, DESCRIPTION, CONTENT, CATEGORY, CREATED_BY_USER_ID, 
                       CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM TEMPLATES
                WHERE IS_ACTIVE = 1
                ORDER BY NAME";

            return await ExecuteReaderAsync(query, MapTemplate);
        }

        public async Task<IEnumerable<Template>> GetByCategoryAsync(string category)
        {
            const string query = @"
                SELECT ID, NAME, DESCRIPTION, CONTENT, CATEGORY, CREATED_BY_USER_ID, 
                       CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM TEMPLATES
                WHERE CATEGORY = :category
                ORDER BY NAME";

            return await ExecuteReaderAsync(query, MapTemplate, CreateParameter(":category", category));
        }

        public async Task<Template> CreateAsync(Template template)
        {
            const string query = @"
                INSERT INTO TEMPLATES (NAME, DESCRIPTION, CONTENT, CATEGORY, CREATED_BY_USER_ID, 
                                      CREATED_AT, IS_ACTIVE)
                VALUES (:name, :description, :content, :category, :createdByUserId, 
                        CURRENT_TIMESTAMP, :isActive)";

            await ExecuteNonQueryAsync(query,
                CreateParameter(":name", template.Name),
                CreateParameter(":description", template.Description),
                CreateParameter(":content", template.Content),
                CreateParameter(":category", template.Category),
                CreateParameter(":createdByUserId", template.CreatedByUserId),
                CreateParameter(":isActive", template.IsActive ? 1 : 0));

            var createdTemplate = await GetByIdAsync(template.Id);
            return createdTemplate ?? throw new InvalidOperationException("Failed to create template");
        }

        public async Task<Template> UpdateAsync(Template template)
        {
            const string query = @"
                UPDATE TEMPLATES
                SET NAME = :name,
                    DESCRIPTION = :description,
                    CONTENT = :content,
                    CATEGORY = :category,
                    UPDATED_AT = CURRENT_TIMESTAMP,
                    IS_ACTIVE = :isActive
                WHERE ID = :id";

            await ExecuteNonQueryAsync(query,
                CreateParameter(":id", template.Id),
                CreateParameter(":name", template.Name),
                CreateParameter(":description", template.Description),
                CreateParameter(":content", template.Content),
                CreateParameter(":category", template.Category),
                CreateParameter(":isActive", template.IsActive ? 1 : 0));

            var updatedTemplate = await GetByIdAsync(template.Id);
            return updatedTemplate ?? throw new InvalidOperationException("Failed to update template");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM TEMPLATES WHERE ID = :id";
            var rowsAffected = await ExecuteNonQueryAsync(query, CreateParameter(":id", id));
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string query = "SELECT COUNT(*) FROM TEMPLATES WHERE ID = :id";
            var count = await ExecuteScalarAsync<int>(query, CreateParameter(":id", id));
            return count > 0;
        }

        private static Template MapTemplate(OracleDataReader reader)
        {
            return new Template
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Content = reader.GetString(3),
                Category = reader.GetString(4),
                CreatedByUserId = reader.GetInt32(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                IsActive = reader.GetInt32(8) == 1
            };
        }
    }
}
