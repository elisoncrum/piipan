using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Npgsql;

namespace Piipan.Shared
{
    public class AzurePgConnectionFactory : IDbConnectionFactory
    {
        // Environment variables (and placeholder) established
        // during initial function app provisioning in IaC
        public const string CloudName = "CloudName";
        public const string DatabaseConnectionString = "DatabaseConnectionString";
        public const string PasswordPlaceholder = "{password}";
        public const string GovernmentCloud = "AzureUSGovernment";

        // Resource ids for open source software databases in the public and
        // US government clouds. Set the desired active cloud, then see:
        // `az cloud show --query endpoints.ossrdbmsResourceId`
        public const string CommercialId = "https://ossrdbms-aad.database.windows.net";
        public const string GovermentId = "https://ossrdbms-aad.database.usgovcloudapi.net";

        private readonly AzureServiceTokenProvider _tokenProvider;
        private readonly DbProviderFactory _dbProviderFactory;

        public AzurePgConnectionFactory(
            AzureServiceTokenProvider tokenProvider,
            DbProviderFactory dbProviderFactory)
        {
            _tokenProvider = tokenProvider;
            _dbProviderFactory = dbProviderFactory;
        }

        public async Task<IDbConnection> Build(string database = null)
        {
            var resourceId = CommercialId;
            var cn = Environment.GetEnvironmentVariable(CloudName);
            if (cn == GovernmentCloud) {
                resourceId = GovermentId;
            }

            var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

            if (builder.Password == PasswordPlaceholder)
            {
                var token = await _tokenProvider.GetAccessTokenAsync(resourceId);
                builder.Password = token;
            }

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