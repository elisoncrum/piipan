// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace PiipanMetricsFunctions
{
    public static class BulkUploadMetrics
    {
        /// <summary>
        /// Listens for BulkUpload events when users upload participants;
        /// write meta info to Metrics database
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="log">handle to the function log</param>
        private static string Host = "piipan-metrics-db.postgres.database.azure.com";
        private static string User = "piipanadmin@piipan-metrics-db";
        private static string DBname = "metrics";
        private static string Password = "<dummy>";
        private static string Port = "5432";

        [FunctionName("BulkUploadMetrics")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            log.LogInformation(eventGridEvent.EventTime.ToString());
            var jsondata = JsonConvert.SerializeObject(eventGridEvent.Data);
            var tmp = new { url = "" };
            var data = JsonConvert.DeserializeAnonymousType(jsondata, tmp);

            Regex regex = new Regex("^https://([a-z]+)state");
            Match match = regex.Match(data.url);

            if (match.Success)
            {
                log.LogInformation(match.Groups[1].Value);
            }
            else
            {
                log.LogInformation("no match found");
                return;
            }

            // Database
            // Build connection string using parameters from portal
            string connString =
                String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);
            using (var conn = new NpgsqlConnection(connString))
            {
                log.LogInformation("Opening db connection");
                conn.Open();
                var tx = conn.BeginTransaction();

                using (var command = new NpgsqlCommand("INSERT INTO user_uploads (actor, uploaded_at) VALUES (@actor, @uploaded_at)", conn))
                {
                    command.Parameters.AddWithValue("actor", match.Groups[1].Value);
                    command.Parameters.AddWithValue("uploaded_at", eventGridEvent.EventTime);
                    int nRows = command.ExecuteNonQuery();
                    log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
                }
                tx.Commit();
                conn.Close();
                log.LogInformation("db connection closed");
            }
        }
    }
}
