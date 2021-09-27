using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchServiceTests
    {
        [Fact]
        public void AddRecord_FailsAfterRetryLimit()
        {
            // Arrange
            const int retries = 10;

            var logger = Mock.Of<ILogger<MatchRecordService>>();
            var matchIdService = Mock.Of<IMatchIdService>();
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var uniqueException = new PostgresException("foo", "bar", "baz", PostgresErrorCodes.UniqueViolation);
            var record = new MatchRecordDbo();

            matchRecordDao
                .Setup(m => m.AddRecord(It.IsAny<MatchRecordDbo>()))
                .ThrowsAsync(uniqueException);

            var service = new MatchRecordService(matchRecordDao.Object, matchIdService, logger);

            // Act + Assert
            Assert.ThrowsAsync<PostgresException>(() => service.AddRecord(record));
            matchRecordDao.Verify(m => m.AddRecord(It.IsAny<MatchRecordDbo>()), Times.Exactly(retries));
        }

        [Fact]
        public void AddRecord_ThrowsOtherPostgresExceptions()
        {
            var logger = Mock.Of<ILogger<MatchRecordService>>();
            var matchIdService = Mock.Of<IMatchIdService>();
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var exception = new PostgresException("foo", "bar", "baz", PostgresErrorCodes.SyntaxError);
            var record = new MatchRecordDbo();

            matchRecordDao
                .Setup(m => m.AddRecord(It.IsAny<MatchRecordDbo>()))
                .ThrowsAsync(exception);

            var service = new MatchRecordService(matchRecordDao.Object, matchIdService, logger);

            // Act + Assert
            Assert.ThrowsAsync<PostgresException>(() => service.AddRecord(record));
            matchRecordDao.Verify(m => m.AddRecord(It.IsAny<MatchRecordDbo>()), Times.Once);
        }

        [Fact]
        public async Task AddRecord_ReturnsMatchId()
        {
            // Arrange
            var logger = Mock.Of<ILogger<MatchRecordService>>();
            var matchIdService = new Mock<IMatchIdService>();
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var record = new MatchRecordDbo();
            var id = "foo";

            matchIdService
                .Setup(m => m.GenerateId())
                .Returns(id);

            matchRecordDao
                .Setup(m => m.AddRecord(It.IsAny<MatchRecordDbo>()))
                .ReturnsAsync((MatchRecordDbo r) => r.MatchId);

            var service = new MatchRecordService(matchRecordDao.Object, matchIdService.Object, logger);

            // Act
            var result = await service.AddRecord(record);

            // Act + Assert
            Assert.True(result == id);
        }
    }
}
