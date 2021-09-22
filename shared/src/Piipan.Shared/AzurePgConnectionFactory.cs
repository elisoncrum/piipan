using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Services.AppAuthentication;
using Npgsql;

namespace Piipan.Shared
{
    public class AzurePgConnectionFactory : IDbConnectionFactory
    {
        [ExcludeFromCodeCoverage]
        public IDbConnection Build()
        {
            // Environment variables (and placeholder) established
            // during initial function app provisioning in IaC
            const string CloudName = "CloudName";
            const string DatabaseConnectionString = "DatabaseConnectionString";
            const string PasswordPlaceholder = "{password}";
            const string GovernmentCloud = "AzureUSGovernment";

            // Resource ids for open source software databases in the public and
            // US government clouds. Set the desired active cloud, then see:
            // `az cloud show --query endpoints.ossrdbmsResourceId`
            const string CommercialId = "https://ossrdbms-aad.database.windows.net";
            const string GovermentId = "https://ossrdbms-aad.database.usgovcloudapi.net";

            var resourceId = CommercialId;
            var cn = Environment.GetEnvironmentVariable(CloudName);
            if (cn == GovernmentCloud) {
                resourceId = GovermentId;
            }

            var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

            if (builder.Password == PasswordPlaceholder)
            {
                var provider = new AzureServiceTokenProvider();
                var token = provider.GetAccessTokenAsync(resourceId).GetAwaiter().GetResult();
                builder.Password = token;
            }
            
            var factory = NpgsqlFactory.Instance;
            var connection = factory.CreateConnection();

            connection.ConnectionString = builder.ConnectionString;
            connection.Open();

            return connection;
        }
    }
}