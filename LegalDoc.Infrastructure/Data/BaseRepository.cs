using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;

namespace LegalDoc.Infrastructure.Data
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;
        protected readonly ILogger<BaseRepository> _logger;

        protected BaseRepository(string connectionString, ILogger<BaseRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        protected async Task<T?> ExecuteScalarAsync<T>(string query, params OracleParameter[] parameters)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        var result = await command.ExecuteScalarAsync();
                        return result == null || result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.LogError(ex, "Oracle error executing scalar query: {Query}", query);
                throw;
            }
        }

        protected async Task<int> ExecuteNonQueryAsync(string query, params OracleParameter[] parameters)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        return await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.LogError(ex, "Oracle error executing non-query: {Query}", query);
                throw;
            }
        }

        protected async Task<List<T>> ExecuteReaderAsync<T>(string query, Func<OracleDataReader, T> mapper, params OracleParameter[] parameters)
        {
            var results = new List<T>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(mapper(reader));
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.LogError(ex, "Oracle error executing reader query: {Query}", query);
                throw;
            }
            return results;
        }

        protected async Task<T?> ExecuteReaderSingleAsync<T>(string query, Func<OracleDataReader, T> mapper, params OracleParameter[] parameters)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return mapper(reader);
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                _logger.LogError(ex, "Oracle error executing single reader query: {Query}", query);
                throw;
            }
            return default;
        }

        protected OracleParameter CreateParameter(string name, object? value)
        {
            return new OracleParameter(name, value ?? DBNull.Value);
        }

        protected OracleParameter CreateParameter(string name, OracleDbType type, object? value)
        {
            return new OracleParameter(name, type) { Value = value ?? DBNull.Value };
        }
    }
}
