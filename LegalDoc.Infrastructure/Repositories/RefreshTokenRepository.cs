using System.Data;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Models;
using LegalDoc.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace LegalDoc.Infrastructure.Repositories
{
    public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
    {
        public RefreshTokenRepository(string connectionString, ILogger<RefreshTokenRepository> logger)
            : base(connectionString, logger)
        {
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            const string sql = @"
                INSERT INTO REFRESH_TOKENS (USER_ID, TOKEN, EXPIRES_AT, CREATED_AT, IS_REVOKED)
                VALUES (:UserId, :Token, :ExpiresAt, :CreatedAt, :IsRevoked)
                RETURNING ID INTO :Id";

            var parameters = new[]
            {
                new OracleParameter("UserId", refreshToken.UserId),
                new OracleParameter("Token", refreshToken.Token),
                new OracleParameter("ExpiresAt", refreshToken.ExpiresAt),
                new OracleParameter("CreatedAt", refreshToken.CreatedAt),
                new OracleParameter("IsRevoked", refreshToken.IsRevoked ? 1 : 0),
                new OracleParameter("Id", OracleDbType.Int32) { Direction = ParameterDirection.Output }
            };

            await ExecuteNonQueryAsync(sql, parameters);
            refreshToken.Id = Convert.ToInt32(parameters[5].Value);
            return refreshToken;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            const string sql = @"
                SELECT rt.ID, rt.USER_ID, rt.TOKEN, rt.EXPIRES_AT, rt.CREATED_AT, rt.IS_REVOKED,
                       u.EMAIL, u.FIRST_NAME, u.LAST_NAME, u.ROLE, u.IS_ACTIVE
                FROM REFRESH_TOKENS rt
                INNER JOIN USERS u ON rt.USER_ID = u.ID
                WHERE rt.TOKEN = :Token AND rt.IS_REVOKED = 0";

            var parameters = new[] { new OracleParameter("Token", token) };

            return await ExecuteReaderSingleAsync(sql, reader => new RefreshToken
            {
                Id = reader.GetInt32("ID"),
                UserId = reader.GetInt32("USER_ID"),
                Token = reader.GetString("TOKEN"),
                ExpiresAt = reader.GetDateTime("EXPIRES_AT"),
                CreatedAt = reader.GetDateTime("CREATED_AT"),
                IsRevoked = reader.GetInt32("IS_REVOKED") == 1,
                User = new User
                {
                    Id = reader.GetInt32("USER_ID"),
                    Email = reader.GetString("EMAIL"),
                    FirstName = reader.GetString("FIRST_NAME"),
                    LastName = reader.GetString("LAST_NAME"),
                    Role = Enum.Parse<Core.Enums.UserRole>(reader.GetString("ROLE")),
                    IsActive = reader.GetInt32("IS_ACTIVE") == 1
                }
            }, parameters);
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            const string sql = @"
                UPDATE REFRESH_TOKENS 
                SET IS_REVOKED = 1 
                WHERE TOKEN = :Token AND IS_REVOKED = 0";

            var parameters = new[] { new OracleParameter("Token", token) };
            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            const string sql = @"
                UPDATE REFRESH_TOKENS 
                SET IS_REVOKED = 1 
                WHERE USER_ID = :UserId AND IS_REVOKED = 0";

            var parameters = new[] { new OracleParameter("UserId", userId) };
            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }

        public async Task CleanupExpiredTokensAsync()
        {
            const string sql = @"
                DELETE FROM REFRESH_TOKENS 
                WHERE EXPIRES_AT < :CurrentTime OR IS_REVOKED = 1";

            var parameters = new[] { new OracleParameter("CurrentTime", DateTime.UtcNow) };
            await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}