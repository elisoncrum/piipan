using System;
using System.Threading;
using Piipan.Match.Func.Api.Models;
using Dapper;
using Npgsql;

namespace Piipan.Match.Func.Api.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
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

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("per-state.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");
                conn.Execute(sqltext);
                conn.Execute("INSERT INTO uploads(created_at, publisher) VALUES(now(), current_user)");

                conn.Close();
            }
        }

        public void ClearParticipants()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM participants");

                conn.Close();
            }
        }

        public void Insert(Participant record)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
                DynamicParameters parameters = new DynamicParameters(record);
                parameters.Add("UploadId", lastval);

                conn.Execute(@"
                    INSERT INTO participants(lds_hash, upload_id, case_id, participant_id, benefits_end_date, recent_benefit_months, protect_location)
                    VALUES (@LdsHash, @UploadId, @CaseId, @ParticipantId, @BenefitsEndDate, @RecentBenefitMonths::date[], @ProtectLocation)",
                    parameters);

                conn.Close();
            }
        }
    }
}
