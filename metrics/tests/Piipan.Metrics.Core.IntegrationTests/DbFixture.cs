using System;
using System.Threading;
using Dapper;
using Npgsql;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.IntegrationTests
{
    /// <summary>
    /// Test fixture for metrics API database integration testing.
    /// Creates a fresh metrics database, dropping it when complete
    /// </summary>
    public class DbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public DbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

            Initialize();
            ApplySchema();
        }

        /// <summary>
        /// Ensure the database is able to receive connections before proceeding.
        /// </summary>
        public void Initialize()
        {
            var retries = 10;
            var wait = 2000; // ms

            while (retries >= 0)
            {
                try
                {
                    using (var conn = Factory.CreateConnection())
                    {
                        conn.ConnectionString = ConnectionString;
                        conn.Open();
                        conn.Close();

                        return;
                    }
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    retries--;
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(wait);
                }
            }
        }

        public void Dispose()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participant_uploads");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("metrics.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participant_uploads");
                conn.Execute(sqltext);

                conn.Close();
            }
        }

        protected void Insert(string state, DateTime uploadedAt)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("INSERT INTO participant_uploads(state, uploaded_at) VALUES (@state, @uploadedAt)", new { state = state, uploadedAt = uploadedAt });

                conn.Close();
            }
        }

        protected bool Has(string state, DateTime uploadedAt)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var record = conn.QuerySingle<ParticipantUpload>(@"
                    SELECT 
                        state State,
                        uploaded_at UploadedAt
                    FROM participant_uploads
                    WHERE 
                        lower(state) LIKE @state AND
                        uploaded_at = @uploadedAt",
                    new
                    {
                        state = state,
                        uploadedAt = uploadedAt
                    });

                result = record.State == state && record.UploadedAt == uploadedAt;
            }

            return result;
        }
    }
}
