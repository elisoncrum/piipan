using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Piipan.Etl.Tests
{
    public class BulkUploadTests
    {
        static string LDS_HASH = "04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef";

        static Stream CsvFixture(string[] records, bool includeHeader = true, bool requiredOnly = false)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            if (includeHeader)
            {
                if (requiredOnly)
                {
                    writer.WriteLine("lds_hash,case_id,participant_id");
                }
                else
                {
                    writer.WriteLine("lds_hash,case_id,participant_id,benefits_end_month,recent_benefit_months,protect_location");

                }
            }
            foreach (var record in records)
            {
                writer.WriteLine(record);
            }
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        static Stream BadBlob()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine("foo");
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        static PiiRecord AllFields()
        {
            return new PiiRecord
            {
                LdsHash = LDS_HASH,
                CaseId = "CaseId",
                ParticipantId = "ParticipantId",
                BenefitsEndDate = new DateTime(1970, 1, 1),
                RecentBenefitMonths = new List<DateTime>() {
                  new DateTime(2021, 5, 31),
                  new DateTime(2021, 4, 30),
                  new DateTime(2021, 3, 31)
                },
                ProtectLocation = true
            };
        }

        static PiiRecord OnlyRequiredFields()
        {
            return new PiiRecord
            {
                LdsHash = LDS_HASH,
                CaseId = "CaseId",
                ParticipantId = null,
                BenefitsEndDate = null,
                RecentBenefitMonths = new List<DateTime>()
            };
        }

        static EventGridEvent EventMock()
        {
            var e = Mock.Of<EventGridEvent>();
            // Can't override Data in Setup, just use a real one
            e.Data = new Object();
            return e;
        }

        // Check that the expected message was logged as an error at least once
        static void VerifyLogError(Mock<ILogger> logger, String expected)
        {
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expected),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public void ReadAllFields()
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] {
                $"{LDS_HASH},CaseId,ParticipantId,1970-01,2021-05 2021-04 2021-03,true"
            });

            var records = BulkUpload.Read(stream, logger);
            int count = 0;
            foreach (var record in records)
            {
                Assert.Equal(LDS_HASH, record.LdsHash);
                Assert.Equal("CaseId", record.CaseId);
                Assert.Equal("ParticipantId", record.ParticipantId);
                Assert.Equal(new DateTime(1970, 1, 31), record.BenefitsEndDate);
                Assert.Equal(new DateTime(2021, 5, 31), record.RecentBenefitMonths[0]);
                Assert.Equal(true, record.ProtectLocation);
                count++;
            }
            Assert.Equal(1, count);
        }

        [Fact]
        public void ReadOptionalFields()
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] {
                $"{LDS_HASH},CaseId,ParticipantId,,,,"
            });

            var records = BulkUpload.Read(stream, logger);
            int count = 0;
            foreach (var record in records)
            {
                Assert.Null(record.BenefitsEndDate);
                Assert.Empty(record.RecentBenefitMonths);
                Assert.Null(record.ProtectLocation);
                count++;
            }
            Assert.Equal(1, count);
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,,ParticipantId,,,")] // Missing CaseId
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,,,,")] // Missing ParticipantId
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,foobar,")] // Malformed Benefits End Month
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,foobar,")] // Malformed Recent Benefit Months
        public void ExpectFieldValidationError(String inline)
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] { inline });

            var records = BulkUpload.Read(stream, logger);
            Assert.Throws<CsvHelper.FieldValidationException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,,foobar")] // Malformed Protect Location
        public void ExpectTypeConverterError(String inline)
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] { inline });

            var records = BulkUpload.Read(stream, logger);
            Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,")] // Missing last column
        public void ExpectMissingFieldError(String inline)
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] { inline });

            var records = BulkUpload.Read(stream, logger);
            Assert.Throws<CsvHelper.MissingFieldException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId")]
        public void OnlyRequiredColumns(String inline)
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] { inline }, requiredOnly: true);

            var records = BulkUpload.Read(stream, logger);
            int count = 0;
            foreach (var record in records)
            {
                Assert.Equal(LDS_HASH, record.LdsHash);
                Assert.Equal("CaseId", record.CaseId);
                Assert.Equal("ParticipantId", record.ParticipantId);
                Assert.Null(record.BenefitsEndDate);
                Assert.Null(record.ProtectLocation);
                Assert.Empty(record.RecentBenefitMonths);
                count++;
            }
            Assert.Equal(1, count);
        }

        [Fact]
        public async void CountInserts()
        {
            var logger = Mock.Of<ILogger>();
            var factory = new Mock<DbProviderFactory>() { DefaultValue = DefaultValue.Mock };
            var cmd = new Mock<DbCommand>() { DefaultValue = DefaultValue.Mock };
            factory.Setup(f => f.CreateCommand()).Returns(cmd.Object);

            // Mocks foreign key used in participants table
            cmd.Setup(c => c.ExecuteScalar()).Returns((Int64)1);

            // Mock can't test unique constraint on SSN
            var records = new List<PiiRecord>() {
                AllFields(),
                OnlyRequiredFields(),
            };
            await BulkUpload.Load(records, factory.Object, logger);

            // Row in uploads table + rows in participants table
            cmd.Verify(f => f.ExecuteNonQuery(), Times.Exactly(1 + records.Count));
        }

        [Fact]
        public async void NoInputStream()
        {
            var gridEvent = EventMock();
            var logger = new Mock<ILogger>();

            Stream input = null;
            await BulkUpload.Run(gridEvent, input, logger.Object);
            VerifyLogError(logger, "No input stream was provided");
        }

        [Fact]
        public async void BadInputStream()
        {
            var gridEvent = EventMock();
            var logger = new Mock<ILogger>();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await BulkUpload.Run(gridEvent, BadBlob(), logger.Object);
            });
        }
    }
}
