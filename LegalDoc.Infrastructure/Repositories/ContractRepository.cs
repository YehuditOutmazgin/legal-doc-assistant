using LegalDoc.Core.Models;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Enums;
using LegalDoc.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;

namespace LegalDoc.Infrastructure.Repositories
{
    public class ContractRepository : BaseRepository, IContractRepository
    {
        public ContractRepository(string connectionString, ILogger<ContractRepository> logger)
            : base(connectionString, logger)
        {
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT ID, TITLE, CONTENT, STATUS, CLIENT_ID, TEMPLATE_ID, CREATED_BY_USER_ID, 
                       ASSIGNED_TO_USER_ID, S3_KEY, CREATED_AT, UPDATED_AT, SIGNED_AT, 
                       SIGNED_BY_NAME, NOTES
                FROM CONTRACTS
                WHERE ID = :id";

            return await ExecuteReaderSingleAsync(query, MapContract, CreateParameter(":id", id));
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            const string query = @"
                SELECT ID, TITLE, CONTENT, STATUS, CLIENT_ID, TEMPLATE_ID, CREATED_BY_USER_ID, 
                       ASSIGNED_TO_USER_ID, S3_KEY, CREATED_AT, UPDATED_AT, SIGNED_AT, 
                       SIGNED_BY_NAME, NOTES
                FROM CONTRACTS
                ORDER BY CREATED_AT DESC";

            return await ExecuteReaderAsync(query, MapContract);
        }

        public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
        {
            const string query = @"
                SELECT ID, TITLE, CONTENT, STATUS, CLIENT_ID, TEMPLATE_ID, CREATED_BY_USER_ID, 
                       ASSIGNED_TO_USER_ID, S3_KEY, CREATED_AT, UPDATED_AT, SIGNED_AT, 
                       SIGNED_BY_NAME, NOTES
                FROM CONTRACTS
                WHERE CLIENT_ID = :clientId
                ORDER BY CREATED_AT DESC";

            return await ExecuteReaderAsync(query, MapContract, CreateParameter(":clientId", clientId));
        }

        public async Task<IEnumerable<Contract>> GetByStatusAsync(ContractStatus status)
        {
            const string query = @"
                SELECT ID, TITLE, CONTENT, STATUS, CLIENT_ID, TEMPLATE_ID, CREATED_BY_USER_ID, 
                       ASSIGNED_TO_USER_ID, S3_KEY, CREATED_AT, UPDATED_AT, SIGNED_AT, 
                       SIGNED_BY_NAME, NOTES
                FROM CONTRACTS
                WHERE STATUS = :status
                ORDER BY CREATED_AT DESC";

            return await ExecuteReaderAsync(query, MapContract, CreateParameter(":status", status.ToString()));
        }

        public async Task<IEnumerable<Contract>> GetByUserIdAsync(int userId)
        {
            const string query = @"
                SELECT ID, TITLE, CONTENT, STATUS, CLIENT_ID, TEMPLATE_ID, CREATED_BY_USER_ID, 
                       ASSIGNED_TO_USER_ID, S3_KEY, CREATED_AT, UPDATED_AT, SIGNED_AT, 
                       SIGNED_BY_NAME, NOTES
                FROM CONTRACTS
                WHERE CREATED_BY_USER_ID = :userId OR ASSIGNED_TO_USER_ID = :userId
                ORDER BY CREATED_AT DESC";

            return await ExecuteReaderAsync(query, MapContract, CreateParameter(":userId", userId));
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            const string query = @"
                INSERT INTO CONTRACTS (TITLE, CONTENT, STATUS, CLIENT_ID, TEMPLATE_ID, 
                                      CREATED_BY_USER_ID, ASSIGNED_TO_USER_ID, S3_KEY, 
                                      CREATED_AT, NOTES)
                VALUES (:title, :content, :status, :clientId, :templateId, 
                        :createdByUserId, :assignedToUserId, :s3Key, 
                        CURRENT_TIMESTAMP, :notes)
                RETURNING ID INTO :id";

            var parameters = new[]
            {
                CreateParameter(":title", contract.Title),
                CreateParameter(":content", contract.Content),
                CreateParameter(":status", contract.Status.ToString()),
                CreateParameter(":clientId", contract.ClientId),
                CreateParameter(":templateId", contract.TemplateId),
                CreateParameter(":createdByUserId", contract.CreatedByUserId),
                CreateParameter(":assignedToUserId", contract.AssignedToUserId),
                CreateParameter(":s3Key", contract.S3Key),
                CreateParameter(":notes", contract.Notes),
                new OracleParameter("id", OracleDbType.Decimal) { Direction = System.Data.ParameterDirection.Output }
            };

            ExecuteNonQuerySync(query, parameters);
            contract.Id = ((Oracle.ManagedDataAccess.Types.OracleDecimal)parameters[9].Value).ToInt32();

            return contract;
        }

        public async Task<Contract> UpdateAsync(Contract contract)
        {
            const string query = @"
                UPDATE CONTRACTS
                SET TITLE = :title,
                    CONTENT = :content,
                    STATUS = :status,
                    ASSIGNED_TO_USER_ID = :assignedToUserId,
                    S3_KEY = :s3Key,
                    UPDATED_AT = CURRENT_TIMESTAMP,
                    SIGNED_AT = :signedAt,
                    SIGNED_BY_NAME = :signedByName,
                    NOTES = :notes
                WHERE ID = :id";

            await ExecuteNonQueryAsync(query,
                CreateParameter(":id", contract.Id),
                CreateParameter(":title", contract.Title),
                CreateParameter(":content", contract.Content),
                CreateParameter(":status", contract.Status.ToString()),
                CreateParameter(":assignedToUserId", contract.AssignedToUserId),
                CreateParameter(":s3Key", contract.S3Key),
                CreateParameter(":signedAt", contract.SignedAt),
                CreateParameter(":signedByName", contract.SignedByName),
                CreateParameter(":notes", contract.Notes));

            var updatedContract = await GetByIdAsync(contract.Id);
            return updatedContract ?? throw new InvalidOperationException("Failed to update contract");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM CONTRACTS WHERE ID = :id";
            var rowsAffected = await ExecuteNonQueryAsync(query, CreateParameter(":id", id));
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string query = "SELECT COUNT(*) FROM CONTRACTS WHERE ID = :id";
            var count = await ExecuteScalarAsync<int>(query, CreateParameter(":id", id));
            return count > 0;
        }

        private static Contract MapContract(OracleDataReader reader)
        {
            return new Contract
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Content = reader.GetString(2),
                Status = Enum.Parse<ContractStatus>(reader.GetString(3)),
                ClientId = reader.GetInt32(4),
                TemplateId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                CreatedByUserId = reader.GetInt32(6),
                AssignedToUserId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                S3Key = reader.IsDBNull(8) ? null : reader.GetString(8),
                CreatedAt = reader.GetDateTime(9),
                UpdatedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                SignedAt = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                SignedByName = reader.IsDBNull(12) ? null : reader.GetString(12),
                Notes = reader.IsDBNull(13) ? null : reader.GetString(13)
            };
        }
    }
}
