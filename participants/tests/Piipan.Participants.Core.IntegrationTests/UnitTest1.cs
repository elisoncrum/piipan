using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Dapper;
using Npgsql;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    public class UnitTest1
    {
        private readonly string ConnectionString;
        private readonly NpgsqlFactory Factory;

        public UnitTest1()
        {
            ConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

            Initialize();
            ApplySchema();
        }

        private void Initialize()
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

        private void Dispose()
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

        [Fact]
        public async void Test1()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var dao = new ParticipantDao(conn);

                await dao.AddParticipants(new List<ParticipantDbo>
                {
                    new ParticipantDbo
                    {
                        LdsHash = "5ab2e8ac6897e2211638e39a252882ae60c86a6038edec47f81cfbb55c083dbe2c094f30e2236a182a98baca4c31e539a60cffb14ee4a0fe6ef16dd1094a231b",
                        CaseId = "c",
                        ParticipantId = "p",
                        BenefitsEndDate = DateTime.UtcNow,
                        RecentBenefitMonths = new List<DateTime>(),
                        ProtectLocation = false,
                        UploadId = 1
                    }
                });

                var p = await dao.GetParticipants("5ab2e8ac6897e2211638e39a252882ae60c86a6038edec47f81cfbb55c083dbe2c094f30e2236a182a98baca4c31e539a60cffb14ee4a0fe6ef16dd1094a231b", 1);

                Assert.Equal("p", p.First().ParticipantId);
            }
        }
    }
}
