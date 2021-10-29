using System;
using System.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Metrics.Core.Extensions;
using Piipan.Shared.Authentication;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

[assembly: FunctionsStartup(typeof(Piipan.Metrics.Func.Api.Startup))]

namespace Piipan.Metrics.Func.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IDbConnection>(c => 
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
                    var token = provider.RetrieveAsync(resourceId).GetAwaiter().GetResult();
                    builder.Password = token.Token;
                }

                var factory = NpgsqlFactory.Instance;
                var connection = factory.CreateConnection();
                
                connection.ConnectionString = builder.ConnectionString;
                connection.Open();

                return connection;
            });

            builder.Services.RegisterCoreServices();
        }
    }
}