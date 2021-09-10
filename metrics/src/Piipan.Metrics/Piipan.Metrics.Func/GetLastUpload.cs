using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Metrics.Models;

#nullable enable

namespace Piipan.Metrics.Api
{
    /// <summary>
    /// implements getting latest upload from each state.
    /// </summary>
    public static class GetLastUpload
    {
        /// <summary>
        /// Azure Function implementing getting latest upload from each state.
        /// </summary>
        [FunctionName("GetLastUpload")]
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
                    log
                );
                var meta = new Meta();
                meta.total = data.Count;
                var response = new GetParticipantUploadsResponse
                {
                    Data = data,
                    Meta = meta
                };

                return (ActionResult)new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        public async static Task<List<ParticipantUpload>> ResultsQuery(
            DbProviderFactory factory,
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
                    cmd.CommandText = ResultsQueryString();
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

        public static String ResultsQueryString()
        {
            var statement = @"
                SELECT state, max(uploaded_at) as uploaded_at
                FROM participant_uploads
                GROUP BY state
                ORDER BY uploaded_at ASC
            ;";
            return statement;
        }
    }
}
