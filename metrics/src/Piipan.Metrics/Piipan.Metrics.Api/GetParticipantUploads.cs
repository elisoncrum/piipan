using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Metrics.Api.Serializers;
using Piipan.Metrics.Models;

#nullable enable

namespace Piipan.Metrics.Api
{
    public static class GetParticipantUploads
    {
        [FunctionName("GetParticipantUploads")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            try
            {
                var dbfactory = NpgsqlFactory.Instance;
                var data = await ResultsQuery(
                    dbfactory,
                    req.Query,
                    log
                );
                var meta = new Meta();
                meta.page = StrToIntWithDefault(req.Query["page"], 1);
                meta.perPage = StrToIntWithDefault(req.Query["perPage"], 50);
                meta.total = await TotalQuery(
                    req,
                    dbfactory,
                    log);
                meta.nextPage = NextPageParams(
                    req.Query["state"],
                    meta.page,
                    meta.perPage,
                    meta.total);
                meta.prevPage = PrevPageParams(
                    req.Query["state"],
                    meta.page,
                    meta.perPage,
                    meta.total);
                var response = new ParticipantUploadsResponse(
                    data,
                    meta
                );

                return (ActionResult)new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        public static String? NextPageParams(
            string? state,
            int page,
            int perPage,
            Int64 total)
        {
            string result = "";
            int nextPage = page + 1;
            // if there are next pages to be had
            if (total >= (page * perPage))
            {
                if (!String.IsNullOrEmpty(state))
                    result = QueryHelpers.AddQueryString(result, "state", state);
                result = QueryHelpers.AddQueryString(result, "page", nextPage.ToString());
                result = QueryHelpers.AddQueryString(result, "perPage", perPage.ToString());
            }
            if (String.IsNullOrEmpty(result))
                return null;
            return result;
        }

        public static String? PrevPageParams(
            string? state,
            int page,
            int perPage,
            Int64 total)
        {
            var newPage = page - 1;
            if (newPage <= 0) return null;

            var result = "";
            if (!String.IsNullOrEmpty(state))
                result = QueryHelpers.AddQueryString(result, "state", state);
            result = QueryHelpers.AddQueryString(result, "page", newPage.ToString());
            result = QueryHelpers.AddQueryString(result, "perPage", perPage.ToString());
            return result;
        }

        public async static Task<Int64> TotalQuery(
            HttpRequest req,
            DbProviderFactory factory,
            ILogger log)
        {
            Int64 count = 0;
            string connString = await ConnectionString(log);
            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                log.LogInformation("Opening db connection");
                conn.Open();
                using (var cmd = factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    var text = "SELECT COUNT(*) from participant_uploads";
                    string? state = req.Query["state"];
                    if (!String.IsNullOrEmpty(state))
                        text += $" WHERE lower(state) LIKE @state";
                    cmd.CommandText = text;
                    if (!String.IsNullOrEmpty(state))
                        AddWithValue(cmd, DbType.String, "state", state.ToLower());
                    count = (Int64)cmd.ExecuteScalar();
                }
                conn.Close();
                log.LogInformation("Closed db connection");
            }
            return count;
        }

        public static String ResultsQueryString(IQueryCollection query)
        {
            string? state = query["state"];
            var statement = "SELECT state, uploaded_at FROM participant_uploads";
            if (!String.IsNullOrEmpty(state))
                statement += $" WHERE lower(state) LIKE @state";
            statement += " ORDER BY uploaded_at DESC";
            statement += $" LIMIT @limit";
            statement += $" OFFSET @offset";
            return statement;
        }

        public static int StrToIntWithDefault(string? s, int @default)
        {
            int number;
            if (int.TryParse(s, out number))
                return number;
            return @default;
        }

        public async static Task<List<ParticipantUpload>> ResultsQuery(
            DbProviderFactory factory,
            IQueryCollection query,
            ILogger log)
        {
            List<ParticipantUpload> results = new List<ParticipantUpload>();
            string connString = await ConnectionString(log);
            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                log.LogInformation("Opening db connection");
                conn.Open();
                using (var cmd = factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = ResultsQueryString(query);
                    string? state = query["state"];
                    if (!String.IsNullOrEmpty(state))
                        AddWithValue(cmd, DbType.String, "state", state.ToLower());
                    int limit = StrToIntWithDefault(query["perPage"], 50);
                    int page = StrToIntWithDefault(query["page"], 1);
                    int offset = limit * (page - 1);
                    AddWithValue(cmd, DbType.Int64, "limit", limit);
                    AddWithValue(cmd, DbType.Int64, "offset", offset);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var record = new ParticipantUpload
                        {
                            state = reader[0].ToString(),
                            uploaded_at = Convert.ToDateTime(reader[1])
                        };
                        results.Add(record);
                    }
                }
                conn.Close();
                log.LogInformation("Closed db connection");
            }
            return results;
        }

        internal async static Task<string> ConnectionString(ILogger log)
        {
            // Environment variable (and placeholder) established
            // during initial function app provisioning in IaC
            const string CloudName = "CloudName";
            const string DatabaseConnectionString = "DatabaseConnectionString";
            const string PasswordPlaceholder = "{password}";
            const string GovernmentCloud = "AzureUSGovernment";
            const string secretName = "core-pg-admin";
            const string vaultNameKey = "KeyVaultName";

            string? vaultName = Environment.GetEnvironmentVariable(vaultNameKey);
            var kvUri = $"https://{vaultName}.vault.azure.net";

            var cn = Environment.GetEnvironmentVariable(CloudName);
            if (cn == GovernmentCloud)
            {
                kvUri = $"https://{vaultName}.vault.usgovcloudapi.net";
            }

            var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            if (builder.Password == PasswordPlaceholder)
            {
                var secret = await client.GetSecretAsync(secretName);
                builder.Password = $"{secret.Value.Value}";
            }

            return builder.ConnectionString;
        }

        static void AddWithValue(DbCommand cmd, DbType type, String name, object value)
        {
            var p = cmd.CreateParameter();
            p.DbType = type;
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
