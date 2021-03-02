using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using Piipan.Metrics.Api.Builders;
using Piipan.Metrics.Api.DataAccessObjects;
using System.Data;

[assembly: FunctionsStartup(typeof(Piipan.Metrics.Startup))]

namespace Piipan.Metrics
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            
            builder.Services.AddTransient<IDbConnection>(async (c) => {
                const string DatabaseConnectionString = "DatabaseConnectionString";
                const string PasswordPlaceholder = "{password}";
                const string secretName = "metrics-pg-admin";
                const string vaultName = "metrics-secret-keeper";
                var kvUri = $"https://{vaultName}.vault.azure.net";

                var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

                var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

                if (builder.Password == PasswordPlaceholder)
                {
                    var secret = await client.GetSecretAsync(secretName);
                    builder.Password = $"{secret.Value.Value}";
                }

                return new NpgsqlConnection(builder.ConnectionString);
            });

            builder.Services.AddTransient<IMetaBuilder, MetaBuilder>();
            builder.Services.AddTransient<IParticipantUploadDao, ParticipantUploadDao>();
        }
    }
}