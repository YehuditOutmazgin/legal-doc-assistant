using LegalDoc.Core.Models;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Enums;
using LegalDoc.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;

namespace LegalDoc.Infrastructure.Repositories
{
    public class ClientRepository : BaseRepository, IClientRepository
    {
        public ClientRepository(string connectionString, ILogger<ClientRepository> logger)
            : base(connectionString, logger)
        {
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT ID, NAME, TYPE, EMAIL, PHONE, ADDRESS, COMPANY_REGISTRATION_NUMBER, 
                       CONTACT_PERSON_NAME, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM CLIENTS
                WHERE ID = :id";

            return await ExecuteReaderSingleAsync(query, MapClient, CreateParameter(":id", id));
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            const string query = @"
                SELECT ID, NAME, TYPE, EMAIL, PHONE, ADDRESS, COMPANY_REGISTRATION_NUMBER, 
                       CONTACT_PERSON_NAME, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM CLIENTS
                ORDER BY NAME";

            return await ExecuteReaderAsync(query, MapClient);
        }

        public async Task<IEnumerable<Client>> GetByTypeAsync(ClientType type)
        {
            const string query = @"
                SELECT ID, NAME, TYPE, EMAIL, PHONE, ADDRESS, COMPANY_REGISTRATION_NUMBER, 
                       CONTACT_PERSON_NAME, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM CLIENTS
                WHERE TYPE = :type
                ORDER BY NAME";

            return await ExecuteReaderAsync(query, MapClient, CreateParameter(":type", type.ToString()));
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            const string query = @"
                SELECT ID, NAME, TYPE, EMAIL, PHONE, ADDRESS, COMPANY_REGISTRATION_NUMBER, 
                       CONTACT_PERSON_NAME, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM CLIENTS
                WHERE EMAIL = :email";

            return await ExecuteReaderSingleAsync(query, MapClient, CreateParameter(":email", email));
        }

        public async Task<Client> CreateAsync(Client client)
        {
            const string query = @"
                INSERT INTO CLIENTS (NAME, TYPE, EMAIL, PHONE, ADDRESS, COMPANY_REGISTRATION_NUMBER, 
                                    CONTACT_PERSON_NAME, CREATED_AT, IS_ACTIVE)
                VALUES (:name, :type, :email, :phone, :address, :companyRegNumber, 
                        :contactPersonName, CURRENT_TIMESTAMP, :isActive)";

            await ExecuteNonQueryAsync(query,
                CreateParameter(":name", client.Name),
                CreateParameter(":type", client.Type.ToString()),
                CreateParameter(":email", client.Email),
                CreateParameter(":phone", client.Phone),
                CreateParameter(":address", client.Address),
                CreateParameter(":companyRegNumber", client.CompanyRegistrationNumber),
                CreateParameter(":contactPersonName", client.ContactPersonName),
                CreateParameter(":isActive", client.IsActive ? 1 : 0));

            var createdClient = await GetByEmailAsync(client.Email);
            return createdClient ?? throw new InvalidOperationException("Failed to create client");
        }

        public async Task<Client> UpdateAsync(Client client)
        {
            const string query = @"
                UPDATE CLIENTS
                SET NAME = :name,
                    TYPE = :type,
                    EMAIL = :email,
                    PHONE = :phone,
                    ADDRESS = :address,
                    COMPANY_REGISTRATION_NUMBER = :companyRegNumber,
                    CONTACT_PERSON_NAME = :contactPersonName,
                    UPDATED_AT = CURRENT_TIMESTAMP,
                    IS_ACTIVE = :isActive
                WHERE ID = :id";

            await ExecuteNonQueryAsync(query,
                CreateParameter(":id", client.Id),
                CreateParameter(":name", client.Name),
                CreateParameter(":type", client.Type.ToString()),
                CreateParameter(":email", client.Email),
                CreateParameter(":phone", client.Phone),
                CreateParameter(":address", client.Address),
                CreateParameter(":companyRegNumber", client.CompanyRegistrationNumber),
                CreateParameter(":contactPersonName", client.ContactPersonName),
                CreateParameter(":isActive", client.IsActive ? 1 : 0));

            var updatedClient = await GetByIdAsync(client.Id);
            return updatedClient ?? throw new InvalidOperationException("Failed to update client");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM CLIENTS WHERE ID = :id";
            var rowsAffected = await ExecuteNonQueryAsync(query, CreateParameter(":id", id));
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string query = "SELECT COUNT(*) FROM CLIENTS WHERE ID = :id";
            var count = await ExecuteScalarAsync<int>(query, CreateParameter(":id", id));
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string query = "SELECT COUNT(*) FROM CLIENTS WHERE EMAIL = :email";
            var count = await ExecuteScalarAsync<int>(query, CreateParameter(":email", email));
            return count > 0;
        }

        private static Client MapClient(OracleDataReader reader)
        {
            return new Client
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Type = Enum.Parse<ClientType>(reader.GetString(2)),
                Email = reader.GetString(3),
                Phone = reader.GetString(4),
                Address = reader.GetString(5),
                CompanyRegistrationNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                ContactPersonName = reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt = reader.GetDateTime(8),
                UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                IsActive = reader.GetInt32(10) == 1
            };
        }
    }
}
