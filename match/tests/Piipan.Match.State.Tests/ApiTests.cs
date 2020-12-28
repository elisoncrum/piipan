using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;


namespace Piipan.Match.State.Tests
{
    public class ApiTests
    {
        static String JsonBody(string json)
        {
            var data = new
            {
                query = JsonConvert.DeserializeObject(json)
            };

            return JsonConvert.SerializeObject(data);
        }

        static Mock<HttpRequest> CreateMockRequest(string jsonBody)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(jsonBody);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            return mockRequest;
        }

        // Valid request returns OkResult

        // Non-required and empty/whitespace properties parse to null
        [Theory]
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000-00-0000'}")] // Missing optionals
        [InlineData(@"{last: 'Last', middle: '', first: '', dob: '2020-01-01', ssn: '000-00-0000'}")] // Empty optionals
        [InlineData(@"{last: 'Last', middle: '     ', first: '\n', dob: '2020-01-01', ssn: '000-00-0000'}")] // Whitespace optionals
        public void ExpectEmptyOptionalPropertiesToBeNull(string query)
        {
            // Arrage
            var body = JsonBody(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(body, logger);
            var valid = Api.Validate(request, logger);

            // Assert
            Assert.Null(request.Query.Middle);
            Assert.Null(request.Query.First);
            Assert.True(valid);
        }

        // Malformed data result in null Query
        [Fact]
        public void MalformedDateResultsInQuery()
        {
            // Arrange
            var malformed = "{{";
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(malformed, logger);

            // Assert
            Assert.Null(request.Query);
        }

        // Incomplete data results in null Query
        [Theory]
        [InlineData("")]
        [InlineData(@"{}")]
        [InlineData(@"{first: 'First'}")] // Missing Last, Dob, and Ssn
        [InlineData(@"{last: 'Last'}")] // Missing Dob and Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-01'}")] // Missing Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-1', ssn: '000-00-000'}")] // Invalid Dob DateTime
        [InlineData(@"{last: 'Last', dob: '', ssn: '000-00-000'}")] // Invalid (empty) Dob DateTime
        public void ExpectQueryToBeNull(string query)
        {
            // Arrange
            var body = JsonBody(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(body, logger);

            // Assert
            Assert.Null(request.Query);
        }

        // Incomplete data returns badrequest
        [Theory]
        [InlineData(@"{first: 'First'}")] // Missing Last, Dob, and Ssn
        [InlineData(@"{last: 'Last'}")] // Missing Dob and Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-01'}")] // Missing Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-1', ssn: '000-00-000'}")] // Invalid Dob DateTime
        [InlineData(@"{last: 'Last', dob: '', ssn: '000-00-000'}")] // Invalid (empty) Dob DateTime
        public async void ExpectBadResultFromIncompleteData(string query)
        {
            // Arrange
            var body = JsonBody(query);
            Mock<HttpRequest> mockRequest = CreateMockRequest(body);
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
            var result = response as BadRequestResult;
            Assert.Equal(400, result.StatusCode);
        }

        // Invalid data fails validation
        [Theory]
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000-00-000'}")] // Invalid Ssn format
        [InlineData(@"{last: '', dob: '2020-01-01', ssn: '000-00-0000'}")] // Empty last
        [InlineData(@"{last: '        ', dob: '2020-01-01', ssn: '000-00-0000'}")] // Whitespace last
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000000000'}")] // Invalid Ssn format
        public void ExpectQueryToBeInvalid(string query)
        {
            // Arrange
            var body = JsonBody(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(body, logger);
            var valid = Api.Validate(request, logger);

            // Assert
            Assert.False(valid);
        }

        // Invalid data returns badrequest
        [Theory]
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000-00-000'}")] // Invalid Ssn format
        [InlineData(@"{last: '', dob: '2020-01-01', ssn: '000-00-0000'}")] // Empty last
        [InlineData(@"{last: '        ', dob: '2020-01-01', ssn: '000-00-0000'}")] // Whitespace last
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000000000'}")] // Invalid Ssn format
        public async void ExpectBadResultFromInvalidData(string query)
        {
            // Arrange
            var body = JsonBody(query);
            Mock<HttpRequest> mockRequest = CreateMockRequest(body);
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
            var result = response as BadRequestResult;
            Assert.Equal(400, result.StatusCode);
        }

        // Parsed valid data returns object with matching properties
        [Fact]
        public void RequestObjectPropertiesMatchesRequestJson()
        {
            // Arrange
            var body = JsonBody(@"{last:'Last', first: 'First', middle: 'Middle', dob: '1970-01-01', ssn: '000-00-0000'}");
            var bodyFormatted = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(body), Formatting.Indented);
            var logger = Mock.Of<ILogger>();

            // Act
            MatchQueryRequest request = Api.Parse(body, logger);

            // Assert
            Assert.Equal("Last", request.Query.Last);
            Assert.Equal("First", request.Query.First);
            Assert.Equal("Middle", request.Query.Middle);
            Assert.Equal("000-00-0000", request.Query.Ssn);
            Assert.Equal(new DateTime(1970, 1, 1), request.Query.Dob);
            Assert.Equal(bodyFormatted, request.ToJson());
        }

        // SQL contains null
        [Fact]
        public void SqlHasIsNullCondition()
        {
            // Arrange
            var body = JsonBody(@"{last:'Last', dob: '1970-01-01', ssn: '000-00-0000'}");
            var logger = Mock.Of<ILogger>();

            // Act
            MatchQueryRequest request = Api.Parse(body, logger);
            (var sql, var parameters) = Api.Prepare(request, logger);

            // Assert
            Assert.Contains("middle IS NULL", sql);
            Assert.Contains("first IS NULL", sql);
        }

        // SQL contains strings
        [Fact]
        public void SqlHasEqualCondition()
        {
            // Arrange
            var body = JsonBody(@"{last:'Last', first: 'First', middle: 'Middle', dob: '1970-01-01', ssn: '000-00-0000'}");
            var logger = Mock.Of<ILogger>();

            // Act
            MatchQueryRequest request = Api.Parse(body, logger);
            (var sql, var parameters) = Api.Prepare(request, logger);

            // Assert
            Assert.Contains("middle=@middle", sql);
            Assert.Contains("first=@first", sql);
        }

        [Fact]
        public void PiiRecordHasStateData()
        {
            // Arrange
            Environment.SetEnvironmentVariable("StateName", "Echo Alpha");
            Environment.SetEnvironmentVariable("StateAbbr", "ea");

            // Act
            var record = new PiiRecord
            {
                First = "First",
                Middle = "Middle",
                Last = "Last",
                Dob = new DateTime(1970, 1, 1),
                Ssn = "000-00-0000",
                Exception = "Exception"
            };

            // Assert
            Assert.Equal("ea", record.StateAbbr);
            Assert.Equal("Echo Alpha", record.StateName);
        }

        // SQL contains string

        // [Fact]
        // public void ValidRequestResultsInOk()
        // {
        //     // Arrange
        //     var logger = Mock.Of<ILogger>();
        //     var factory = new Mock<DbProviderFactory>() { DefaultValue = DefaultValue.Mock };
        //     var cmd = new Mock<DbCommand>() { DefaultValue = DefaultValue.Mock };
        //     var conn = new Mock<IDbConnection>() { DefaultValue = DefaultValue.Mock };

        //     factory.Setup(f => f.CreateCommand()).Returns(cmd.Object);

        //     // Mocks upload_id
        //     cmd.Setup(c => c.ExecuteScalar()).Returns((Int32)1);

        //     // Mocks SELECT results
        //     conn.Setup(c => c.Query(It.IsAny<String>(), It.IsAny<Object>(), null, true, null, null)).Returns(new List<PiiRecord>());

        //     // Act


        //     // Assert
        //     Assert.False(true);
        // }
    }
}
