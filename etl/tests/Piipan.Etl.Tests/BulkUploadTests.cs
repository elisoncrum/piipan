using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Piipan.Etl.Tests
{
    public class BulkUploadTests
    {
        static Stream CsvFixture(string[] records, bool includeHeader = true)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            if (includeHeader)
            {
                writer.WriteLine("last,first,middle,dob,ssn,exception");
            }
            foreach (var record in records)
            {
                writer.WriteLine(record);
            }
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        [Fact]
        public void ReadAllFields()
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] {
                "Last,First,Middle,01/01/1970,000-00-0000,Exception"
            });

            var records = BulkUpload.Read(stream, logger);
            foreach (var record in records)
            {
                Assert.Equal("Last", record.Last);
                Assert.Equal("First", record.First);
                Assert.Equal("Middle", record.Middle);
                Assert.Equal(new DateTime(1970, 1, 1), record.Dob);
                Assert.Equal("000-00-0000", record.Ssn);
                Assert.Equal("Exception", record.Exception);
            }
        }

        [Fact]
        public void ReadOptionalFields()
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] {
                "Last,,,01/01/1970,000-00-0000,"
            });

            var records = BulkUpload.Read(stream, logger);
            foreach (var record in records)
            {
                Assert.Null(record.First);
                Assert.Null(record.Middle);
                Assert.Null(record.Exception);
            }
        }

        [Theory]
        [InlineData(",,,01/01/1970,000-00-0000,")] // Missing last name
        [InlineData("Last,,,01/01/1970,,")] // Missing SSN
        [InlineData("Last,,,01/01/1970,000000000,")] // Malformed SSN
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
        [InlineData("Last,,,,000-00-0000,")] // Missing DOB
        [InlineData("Last,,,02/31/1970,000-00-0000,")] // Invalid DOB
        public void ExpectReadErrror(String inline)
        {
            var logger = Mock.Of<ILogger>();
            var stream = CsvFixture(new string[] { inline });

            var records = BulkUpload.Read(stream, logger);
            Assert.Throws<CsvHelper.ReaderException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }
    }
}
