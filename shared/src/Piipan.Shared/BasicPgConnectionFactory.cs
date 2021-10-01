using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace Piipan.Shared
{
    public class BasicPgConnectionFactory : IDbConnectionFactory
    {
        public readonly DbProviderFactory _dbProviderFactory;
        public const string DatabaseConnectionString = "DatabaseConnectionString";

        public BasicPgConnectionFactory(DbProviderFactory dbProviderFactory)
        {
            _dbProviderFactory = dbProviderFactory;
        }

        public async Task<IDbConnection> Build(string database = null)
        {
            if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable(DatabaseConnectionString)))
            {
                throw new ArgumentException($"{DatabaseConnectionString} env variable must be set!");
            }
            
            var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

            if (!String.IsNullOrEmpty(database))
            {
                builder.Database = database;
            }

            var connection = _dbProviderFactory.CreateConnection();

            connection.ConnectionString = builder.ConnectionString;
            await connection.OpenAsync();
            
            return connection;
        }
    }
}