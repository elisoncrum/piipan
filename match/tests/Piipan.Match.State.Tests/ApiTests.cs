using System;
using System.Collections.Generic;
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
        void SetEnvironment()
        {
            Environment.SetEnvironmentVariable("StateName", "Echo Alpha");
            Environment.SetEnvironmentVariable("StateAbbr", "ea");
        }

        static PiiRecord FullRecord()
        {
            return new PiiRecord
            {
                First = "First",
                Middle = "Middle",
                Last = "Last",
                Dob = new DateTime(1970, 1, 1),
                Ssn = "000-00-0000",
                Exception = "Exception"
            };
        }

        static String JsonBody(string json)
        {
            var data = new
            {
                query = JsonConvert.DeserializeObject(json)
            };

            return JsonConvert.SerializeObject(data);
        }

        static Mock<HttpRequest> MockRequest(string jsonBody)
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
        [Theory]
        [InlineData("{{")]
        [InlineData("<xml>")]
        public void ExpectMalformedDataResultsInNullQuery(string query)
        {
            // Arrange
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(query, logger);

            // Assert
            Assert.Null(request.Query);
        }

        // Malformed data results in BadRequest
        [Theory]
        [InlineData("{{")]
        [InlineData("<xml>")]
        public async void ExpectMalformedDataResultsInBadRequest(string query)
        {
            // Arrange
            Mock<HttpRequest> mockRequest = MockRequest(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
            var result = response as BadRequestResult;
            Assert.Equal(400, result.StatusCode);
        }

        // Incomplete data results in null Query
        [Theory]
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
        // Note: date validation happens in `Api.Parse` not `Api.Validate`
        [Theory]
        [InlineData("")]
        [InlineData(@"{first: 'First'}")] // Missing Last, Dob, and Ssn
        [InlineData(@"{last: 'Last'}")] // Missing Dob and Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-01'}")] // Missing Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-1', ssn: '000-00-000'}")] // Invalid Dob DateTime
        [InlineData(@"{last: 'Last', dob: '', ssn: '000-00-000'}")] // Empty Dob DateTime
        public async void ExpectBadResultFromIncompleteData(string query)
        {
            // Arrange
            Mock<HttpRequest> mockRequest = MockRequest(JsonBody(query));
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
            var result = response as BadRequestResult;
            Assert.Equal(400, result.StatusCode);
        }

        // Invalid data fails validation
        // Note: date validation happens in `Api.Parse` not `Api.Validate`
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
            Mock<HttpRequest> mockRequest = MockRequest(JsonBody(query));
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
        public void RequestObjectPropertiesMatchRequestJson()
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
            SetEnvironment();
            var record = FullRecord();

            // Assert
            Assert.Equal("ea", record.StateAbbr);
            Assert.Equal("Echo Alpha", record.StateName);
        }

        [Fact]
        public void PiiRecordJsonMatchesObject()
        {
            // Arrange
            SetEnvironment();
            var record = FullRecord();
            var expected = "{\n  \"last\": \"Last\",\n  \"first\": \"First\",\n  \"middle\": \"Middle\",\n  \"ssn\": \"000-00-0000\",\n  \"dob\": \"1970-01-01\",\n  \"exception\": \"Exception\",\n  \"state_name\": \"Echo Alpha\",\n  \"state_abbr\": \"ea\"\n}";

            // Assert
            Assert.Equal(expected, record.ToJson());
        }

        [Fact]
        public void MatchResponseJsonMatchesObject()
        {
            // Arrange
            SetEnvironment();
            var record = FullRecord();
            var response = new MatchQueryResponse
            {
                Matches = new List<PiiRecord>()
            };
            var expected = "{\n  \"matches\": [\n    {" +
                    "\n      \"last\": \"Last\",\n      \"first\": \"First\",\n      \"middle\": \"Middle\",\n      \"ssn\": \"000-00-0000\",\n      \"dob\": \"1970-01-01\",\n      \"exception\": \"Exception\",\n      \"state_name\": \"Echo Alpha\",\n      \"state_abbr\": \"ea\"" +
                    "\n    }\n  ]\n}";

            // Act
            response.Matches.Add(record);

            // Assert
            Assert.Equal(expected, response.ToJson());
        }

        // XXX Connection string contains appropriate config
        // XXX Valid request returns JsonResult
    }
}
