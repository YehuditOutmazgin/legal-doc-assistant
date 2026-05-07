using LegalDoc.Core.Models;
using LegalDoc.Core.Interfaces;
using LegalDoc.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;

namespace LegalDoc.Infrastructure.Repositories
{
    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        public AuditLogRepository(string connectionString, ILogger<AuditLogRepository> logger)
            : base(connectionString, logger)
        {
        }

        public async Task LogAsync(int contractId, int userId, string action, string? details = null)
        {
            const string query = @"
                INSERT INTO AUDIT_LOGS (CONTRACT_ID, USER_ID, ACTION, DETAILS, TIMESTAMP)
                VALUES (:contractId, :userId, :action, :details, CURRENT_TIMESTAMP)";

            await ExecuteNonQueryAsync(query,
                CreateParameter(":contractId", contractId),
                CreateParameter(":userId", userId),
                CreateParameter(":action", action),
                CreateParameter(":details", details));
        }

        public async Task<IEnumerable<AuditLog>> GetByContractIdAsync(int contractId)
        {
            const string query = @"
                SELECT ID, CONTRACT_ID, USER_ID, ACTION, DETAILS, TIMESTAMP
                FROM AUDIT_LOGS
                WHERE CONTRACT_ID = :contractId
                ORDER BY TIMESTAMP DESC";

            return await ExecuteReaderAsync(query, MapAuditLog, CreateParameter(":contractId", contractId));
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId)
        {
            const string query = @"
                SELECT ID, CONTRACT_ID, USER_ID, ACTION, DETAILS, TIMESTAMP
                FROM AUDIT_LOGS
                WHERE USER_ID = :userId
                ORDER BY TIMESTAMP DESC";

            return await ExecuteReaderAsync(query, MapAuditLog, CreateParameter(":userId", userId));
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50)
        {
            const string query = @"
                SELECT ID, CONTRACT_ID, USER_ID, ACTION, DETAILS, TIMESTAMP
                FROM AUDIT_LOGS
                ORDER BY TIMESTAMP DESC
                FETCH FIRST :count ROWS ONLY";

            return await ExecuteReaderAsync(query, MapAuditLog, CreateParameter(":count", count));
        }

        private static AuditLog MapAuditLog(OracleDataReader reader)
        {
            return new AuditLog
            {
                Id = reader.GetInt32(0),
                ContractId = reader.GetInt32(1),
                UserId = reader.GetInt32(2),
                Action = reader.GetString(3),
                Details = reader.IsDBNull(4) ? null : reader.GetString(4),
                Timestamp = reader.GetDateTime(5)
            };
        }
    }
}
