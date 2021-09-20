using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Dapper;
using Npgsql;
using Xunit;
using Newtonsoft.Json;

namespace Piipan.Participants.Core.IntegrationTests
{
    public class ParticipantDaoTests : DbFixture
    {
        private string RandomHashString()
        {
            SHA512 sha = SHA512Managed.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private IEnumerable<ParticipantDbo> RandomParticipants(int n)
        {
            var result = new List<ParticipantDbo>();

            for (int i = 0; i < n; i++)
            {
                result.Add(new ParticipantDbo
                {
                    LdsHash = RandomHashString(),
                    CaseId = Guid.NewGuid().ToString(),
                    ParticipantId = Guid.NewGuid().ToString(),
                    BenefitsEndDate = DateTime.UtcNow.Date,
                    RecentBenefitMonths = new List<DateTime>(),
                    ProtectLocation = (new Random()).Next(2) == 0,
                    UploadId = GetLastUploadId()
                });
            }

            return result;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void AddParticipants(int nParticipants)
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearParticipants();

                var dao = new ParticipantDao(conn);
                var participants = RandomParticipants(nParticipants);

                // Act
                await dao.AddParticipants(participants);

                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p));
                });
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(20)]
        public async void GetParticipants(int nMatches)
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearParticipants();

                var randoms = RandomParticipants(nMatches);
                var participants = randoms.ToList().Select(p =>
                {
                    return new ParticipantDbo
                    {
                        // make the hashes and upload id match for all of them
                        LdsHash = randoms.First().LdsHash,
                        CaseId = p.CaseId,
                        ParticipantId = p.ParticipantId,
                        BenefitsEndDate = p.BenefitsEndDate,
                        RecentBenefitMonths = p.RecentBenefitMonths,
                        ProtectLocation = p.ProtectLocation,
                        UploadId = randoms.First().UploadId
                    };
                });

                participants.ToList().ForEach(p => Insert(p));

                var dao = new ParticipantDao(conn);

                // Act
                var matches = await dao.GetParticipants(randoms.First().LdsHash, randoms.First().UploadId);

                // Assert
                Assert.True(participants.OrderBy(p => p.CaseId).SequenceEqual(matches.OrderBy(p => p.CaseId)));
            }
        }
    }
}
