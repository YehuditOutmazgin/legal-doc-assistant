using LegalDoc.Core.Models;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Enums;
using LegalDoc.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;

namespace LegalDoc.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(string connectionString, ILogger<UserRepository> logger)
            : base(connectionString, logger)
        {
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT ID, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM USERS
                WHERE ID = :id";

            return await ExecuteReaderSingleAsync(query, MapUser, CreateParameter(":id", id));
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            const string query = @"
                SELECT ID, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM USERS
                ORDER BY ID";

            return await ExecuteReaderAsync(query, MapUser);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string query = @"
                SELECT ID, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM USERS
                WHERE EMAIL = :email";

            return await ExecuteReaderSingleAsync(query, MapUser, CreateParameter(":email", email));
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            const string query = @"
                SELECT ID, EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, UPDATED_AT, IS_ACTIVE
                FROM USERS
                WHERE ROLE = :role
                ORDER BY FIRST_NAME, LAST_NAME";

            return await ExecuteReaderAsync(query, MapUser, CreateParameter(":role", role.ToString()));
        }

        public async Task<User> CreateAsync(User user)
        {
            const string query = @"
                INSERT INTO USERS (EMAIL, PASSWORD_HASH, FIRST_NAME, LAST_NAME, ROLE, CREATED_AT, IS_ACTIVE)
                VALUES (:email, :passwordHash, :firstName, :lastName, :role, CURRENT_TIMESTAMP, :isActive)
                RETURNING ID INTO :id";

            var parameters = new[]
            {
                CreateParameter(":email", user.Email),
                CreateParameter(":passwordHash", user.PasswordHash ?? string.Empty),
                CreateParameter(":firstName", user.FirstName),
                CreateParameter(":lastName", user.LastName),
                CreateParameter(":role", user.Role.ToString()),
                CreateParameter(":isActive", user.IsActive ? 1 : 0),
                new OracleParameter("id", OracleDbType.Decimal) { Direction = System.Data.ParameterDirection.Output }
            };

            ExecuteNonQuerySync(query, parameters);
            user.Id = ((Oracle.ManagedDataAccess.Types.OracleDecimal)parameters[6].Value).ToInt32();

            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            const string query = @"
                UPDATE USERS
                SET EMAIL = :email,
                    PASSWORD_HASH = :passwordHash,
                    FIRST_NAME = :firstName,
                    LAST_NAME = :lastName,
                    ROLE = :role,
                    UPDATED_AT = CURRENT_TIMESTAMP,
                    IS_ACTIVE = :isActive
                WHERE ID = :id";

            await ExecuteNonQueryAsync(query,
                CreateParameter(":id", user.Id),
                CreateParameter(":email", user.Email),
                CreateParameter(":passwordHash", user.PasswordHash ?? string.Empty),
                CreateParameter(":firstName", user.FirstName),
                CreateParameter(":lastName", user.LastName),
                CreateParameter(":role", user.Role.ToString()),
                CreateParameter(":isActive", user.IsActive ? 1 : 0));

            var updatedUser = await GetByIdAsync(user.Id);
            return updatedUser ?? throw new InvalidOperationException("Failed to update user");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM USERS WHERE ID = :id";
            var rowsAffected = await ExecuteNonQueryAsync(query, CreateParameter(":id", id));
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            const string query = "SELECT COUNT(*) FROM USERS WHERE ID = :id";
            var count = await ExecuteScalarAsync<int>(query, CreateParameter(":id", id));
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string query = "SELECT COUNT(*) FROM USERS WHERE EMAIL = :email";
            var count = await ExecuteScalarAsync<int>(query, CreateParameter(":email", email));
            return count > 0;
        }

        private static User MapUser(OracleDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FirstName = reader.GetString(3),
                LastName = reader.GetString(4),
                Role = Enum.Parse<UserRole>(reader.GetString(5)),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                IsActive = reader.GetInt32(8) == 1
            };
        }
    }
}
