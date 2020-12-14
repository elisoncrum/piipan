// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
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
                    var records = Read(input, log);
                    Load(records, NpgsqlFactory.Instance, log);
                }
                else
                {
                    // Can get here if Function does not have
                    // permission to access blob URL
                    log.LogError("No input stream was provided");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        internal static IEnumerable<PiiRecord> Read(Stream input, ILogger log)
        {
            var reader = new StreamReader(input);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.TrimOptions = TrimOptions.Trim;
            csv.Configuration.RegisterClassMap<PiiRecordMap>();

            // Yields records as it is iterated over
            return csv.GetRecords<PiiRecord>();
        }

        internal static void Load(IEnumerable<PiiRecord> records, DbProviderFactory factory, ILogger log)
        {
            var connString = Environment.GetEnvironmentVariable("DatabaseConnectionString");

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                conn.Open();

                // Assumes we want to process in an all-or-nothing fashion;
                // i.e., a processing error in one record spoils the whole batch
                var tx = conn.BeginTransaction();

                using (var cmd = factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO uploads (created_at, publisher) VALUES(now(), current_user)";
                    cmd.ExecuteNonQuery();
                }

                Int64 lastval = 0;
                using (var cmd = factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT lastval()";
                    lastval = (Int64)cmd.ExecuteScalar();
                }

                foreach (var record in records)
                {
                    using (var cmd = factory.CreateCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO participants (last, first, middle, dob, ssn, exception, upload_id) " +
                            "VALUES (@last, @first, @middle, @dob, @ssn, @exception, @upload_id)";

                        AddWithValue(cmd, DbType.String, "last", record.Last);
                        AddWithValue(cmd, DbType.String, "first", (object)record.First ?? DBNull.Value);
                        AddWithValue(cmd, DbType.String, "middle", (object)record.Middle ?? DBNull.Value);
                        AddWithValue(cmd, DbType.DateTime, "dob", record.Dob);
                        AddWithValue(cmd, DbType.String, "ssn", record.Ssn);
                        AddWithValue(cmd, DbType.String, "exception", (object)record.Exception ?? DBNull.Value);
                        AddWithValue(cmd, DbType.Int64, "upload_id", lastval);

                        cmd.ExecuteNonQuery();
                    }
                }
                tx.Commit();
                conn.Close();
            }
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
