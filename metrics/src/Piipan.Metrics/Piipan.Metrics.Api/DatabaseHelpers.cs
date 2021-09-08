using System;
using System.Threading.Tasks;
using Npgsql;
using Piipan.Shared.Authentication;

namespace Piipan.Metrics.Api
{
    /// <summary>
    /// Holds common functionality for accessing database
    /// </summary>
    public class DatabaseHelpers
    {
        public async static Task<string> ConnectionString()
        {
            // Environment variable (and placeholder) established
            // during initial function app provisioning in IaC
            const string CloudName = "CloudName";
            const string GovernmentCloud = "AzureUSGovernment";
            const string DatabaseConnectionString = "DatabaseConnectionString";
            const string PasswordPlaceholder = "{password}";

            // Resource ids for open source software databases in the public and
            // US government clouds. Set the desired active cloud, then see:
            // `az cloud show --query endpoints.ossrdbmsResourceId`
            const string CommercialId = "https://ossrdbms-aad.database.windows.net";
            const string GovermentId = "https://ossrdbms-aad.database.usgovcloudapi.net";

            var resourceId = CommercialId;
            var cn = Environment.GetEnvironmentVariable(CloudName);
            if (cn == GovernmentCloud)
            {
                resourceId = GovermentId;
            }

            var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

            if (builder.Password == PasswordPlaceholder)
            {
                var provider = new EasyAuthTokenProvider();
                var token = await provider.RetrieveAsync(resourceId);
                builder.Password = token.Token;
            }

            return builder.ConnectionString;
        }
    }
}
