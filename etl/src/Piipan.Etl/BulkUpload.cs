// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Piipan.Etl
{
    /// <summary>
    /// Azure Function implementing basic Extract-Transform-Load of piipan
    /// bulk import CSV files via Storage Containers, Event Grid, and
    /// PostgreSQL.
    /// </summary>
    public static class BulkUpload
    {
        static string GetBlobNameFromUrl(string bloblUrl)
        {
            var uri = new Uri(bloblUrl);
            var blobClient = new BlobClient(uri);
            return blobClient.Name;
        }

        /// <summary>
        /// Entry point for the state-specific Azure Function instance
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="input">handle to CSV file uploaded to a state-specific container</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// The function is expected to be executing as a managed identity that has read access
        /// to the per-state storage account and write access to the per-state database.
        /// </remarks>
        [FunctionName("BulkUpload")]
        public static void Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [Blob("{data.url}", FileAccess.Read, Connection = "BlobStorageConnectionString")] Stream input,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());

            try
            {
                if (input != null)
                {
                    var createdEvent = ((JObject)eventGridEvent.Data).ToObject<StorageBlobCreatedEventData>();
                    var blobName = GetBlobNameFromUrl(createdEvent.Url);
                    log.LogDebug($"Extracting records from {blobName}");

                    using (var reader = new StreamReader(input))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.TrimOptions = TrimOptions.Trim;
                        csv.Configuration.RegisterClassMap<PiiRecordMap>();
                        
                        // Yields records as it is iterated over
                        var records = csv.GetRecords<PiiRecord>();
                        Load(records, log);
                    }
                }
                else
                {
                    log.LogError("No input stream was provided");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        static void Load(IEnumerable<PiiRecord> records, ILogger log)
        {
            var connString = Environment.GetEnvironmentVariable("DatabaseConnectionString");

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Assumes we want to process in an all-or-nothing fashion;
                // i.e., a processing error in one record spoils the whole batch
                var tx = conn.BeginTransaction();

                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO uploads (created_at, publisher) VALUES(now(), current_user)", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                Int64 lastval = 0;
                using (var cmd = new NpgsqlCommand("SELECT lastval()", conn))
                {
                    lastval = (Int64)cmd.ExecuteScalar();
                }

                foreach (var record in records)
                {
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO participants (last, first, middle, dob, ssn, exception, upload_id) " +
                        "VALUES (@last, @first, @middle, @dob, @ssn, @exception, @upload_id)", conn))
                    {
                        cmd.Parameters.AddWithValue("last", record.Last);
                        cmd.Parameters.AddWithValue("first", (object)record.First ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("middle", (object)record.Middle ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("dob", record.Dob);
                        cmd.Parameters.AddWithValue("ssn", record.Ssn);
                        cmd.Parameters.AddWithValue("exception", (object)record.Exception ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("upload_id", lastval);
                        cmd.ExecuteNonQuery();
                    }
                }
                tx.Commit();
                conn.Close();
            }
        }
    }
}
