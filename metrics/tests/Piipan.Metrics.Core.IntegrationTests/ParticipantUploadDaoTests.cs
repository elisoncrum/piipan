using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Piipan.Shared.Database;
using Piipan.Metrics.Core.DataAccessObjects;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace Piipan.Metrics.Core.IntegrationTests
{
    

    public class ParticipantUploadDaoTests : DbFixture
    {
        private IDbConnectionFactory<MetricsDb> DbConnFactory()
        {
            var factory = new Mock<IDbConnectionFactory<MetricsDb>>();
            factory
                .Setup(m => m.Build(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    var conn = Factory.CreateConnection();
                    conn.ConnectionString = ConnectionString;
                    conn.Open();
                    return conn;
                });

            return factory.Object;
        }

        private string RandomState()
        {
            return Guid.NewGuid().ToString().Substring(0, 2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public async Task GetUploadCount_ReturnsExpected(long expectedCount)
        {
            // Arrange
            for (var i = 0; i < expectedCount; i++)
            {
                Insert(RandomState(), DateTime.Now);
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var count = await dao.GetUploadCount(null);

            // Assert
            Assert.Equal(expectedCount, count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public async Task GetUploadCountByState_ReturnsExpected(long expectedCount)
        {
            // Arrange
            for (var i = 0; i < expectedCount; i++)
            {
                Insert("ea", DateTime.Now);
                Insert("eb", DateTime.Now);
                Insert("ec", DateTime.Now);
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var eaCount = await dao.GetUploadCount("ea");
            var ebCount = await dao.GetUploadCount("eb");
            var ecCount = await dao.GetUploadCount("ec");

            // Assert
            Assert.Equal(expectedCount, eaCount);
            Assert.Equal(expectedCount, ebCount);
            Assert.Equal(expectedCount, ecCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public async Task GetUploads_ReturnsCorrectCount(long count)
        {
            // Arrange
            for (var i = 0; i < count; i++)
            {
                Insert("ea", DateTime.Now);
                Insert("eb", DateTime.Now);
                Insert("ec", DateTime.Now);
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var eaUploads = await dao.GetUploads("ea", 100);
            var ebUploads = await dao.GetUploads("eb", 100);
            var ecUploads = await dao.GetUploads("ec", 100);

            // Assert
            Assert.Equal(count, eaUploads.Count());
            Assert.Equal(count, ebUploads.Count());
            Assert.Equal(count, ecUploads.Count());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]        
        public async Task GetUploads_LimitingWorks(int limit)
        {
            // Arrange
            for (var i = 0; i < 100; i++)
            {
                Insert("ea", DateTime.Now);
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var eaUploads = await dao.GetUploads("ea", limit);

            // Assert
            Assert.Equal(limit, eaUploads.Count());
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]        
        public async Task GetUploads_OffsettingWorks(int offset)
        {
            // Arrange
            for (var i = 0; i < 100; i++)
            {
                Insert("ea", DateTime.Now);
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());
            // Act
            var eaUploads = await dao.GetUploads("ea", 100, offset);

            // Assert
            Assert.Equal(100 - offset, eaUploads.Count());
        }

        [Fact]
        public async Task GetLatestUploadsByState_ReturnsExpected()
        {
            // Arrange
            DateTime latestEA = new DateTime(2021, 1, 1, 5, 10, 0);
            DateTime latestEB = new DateTime(2021, 2, 2, 5, 10, 0);
            DateTime latestEC = new DateTime(2021, 3, 3, 5, 10, 0);
            for (var i = 0; i < 100; i++)
            {
                latestEA = latestEA + TimeSpan.FromSeconds(i);
                latestEB = latestEB + TimeSpan.FromSeconds(i);
                latestEC = latestEC + TimeSpan.FromSeconds(i);
                Insert("ea", latestEA);
                Insert("eb", latestEB);
                Insert("ec", latestEC);
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var latest = await dao.GetLatestUploadsByState();

            // Assert
            Assert.Equal(3, latest.Count());
            Assert.Single(latest, u => u.State == "ea" && u.UploadedAt.Equals(latestEA));
            Assert.Single(latest, u => u.State == "eb" && u.UploadedAt.Equals(latestEB));
            Assert.Single(latest, u => u.State == "ec" && u.UploadedAt.Equals(latestEC));
        }

        [Fact]
        public async Task AddUpload_InsertsRecord()
        {
            // Arrange
            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());
            var uploadedAt = new DateTime(2021, 1, 1, 5, 10, 0);

            // Act
            await dao.AddUpload("ea", uploadedAt);

            // Assert
            Assert.True(Has("ea", uploadedAt));
        }
    }
}