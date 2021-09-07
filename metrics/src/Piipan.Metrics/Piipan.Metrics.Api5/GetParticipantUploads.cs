using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Npgsql;
using Piipan.Metrics.Api5.Serializers;
using Piipan.Metrics.Models;

namespace Piipan.Metrics.Api5
{
    public static class GetParticipantUploads
    {
        [Function("GetParticipantUploads")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("GetParticipantUploads");
            try
            {
                var dbfactory = NpgsqlFactory.Instance;
                var query = QueryHelpers.ParseQuery(req.Url.Query);
                var data = await ResultsQuery(
                    dbfactory,
                    query,
                    log
                );
                var meta = new Meta();
                StringValues page = "";
                query.TryGetValue("page", out page);
                StringValues perPage = "";
                query.TryGetValue("perPage", out perPage);
                StringValues state = "";
                query.TryGetValue("state", out state);
                meta.page = StrToIntWithDefault(page, 1);
                meta.perPage = StrToIntWithDefault(perPage, 50);
                meta.total = await TotalQuery(
                    query,
                    dbfactory,
                    log);
                meta.nextPage = NextPageParams(
                    state,
                    meta.page,
                    meta.perPage,
                    meta.total);
                meta.prevPage = PrevPageParams(
                    state,
                    meta.page,
                    meta.perPage,
                    meta.total);
                var response = new ParticipantUploadsResponse(
                    data,
                    meta
                );

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(response);

                return res;
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
            Dictionary<String,StringValues> query,
            DbProviderFactory factory,
            ILogger log)
        {
            Int64 count = 0;
            string connString = await DatabaseHelpers.ConnectionString();
            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                log.LogInformation("Opening db connection");
                conn.Open();
                using (var cmd = factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    var text = "SELECT COUNT(*) from participant_uploads";
                    StringValues state = "";
                    query.TryGetValue("state", out state);
                    if (!String.IsNullOrEmpty(state))
                        text += $" WHERE lower(state) LIKE @state";
                    cmd.CommandText = text;
                    if (!String.IsNullOrEmpty(state))
                        AddWithValue(cmd, DbType.String, "state", state.ToString().ToLower());
                    count = (Int64)cmd.ExecuteScalar();
                }
                conn.Close();
                log.LogInformation("Closed db connection");
            }
            return count;
        }

        public static String ResultsQueryString(Dictionary<String,StringValues> query)
        {
            StringValues state = "";
            var statement = "SELECT state, uploaded_at FROM participant_uploads";
            if (query.TryGetValue("state", out state))
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
            Dictionary<String,StringValues> query,
            ILogger log)
        {
            List<ParticipantUpload> results = new List<ParticipantUpload>();
            string connString = await DatabaseHelpers.ConnectionString();
            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                log.LogInformation("Opening db connection");
                conn.Open();
                using (var cmd = factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = ResultsQueryString(query);
                    StringValues state = "";
                    if (query.TryGetValue("state", out state))
                        AddWithValue(cmd, DbType.String, "state", state.ToString().ToLower());
                    StringValues perPage = "";
                    query.TryGetValue("perPage", out perPage);
                    int limit = StrToIntWithDefault(perPage, 50);
                    StringValues pageStr = "";
                    query.TryGetValue("page", out pageStr);
                    int page = StrToIntWithDefault(pageStr, 1);
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
