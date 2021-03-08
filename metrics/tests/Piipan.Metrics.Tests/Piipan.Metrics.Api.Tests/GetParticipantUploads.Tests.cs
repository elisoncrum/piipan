using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using Piipan.Metrics.Api;
using Piipan.Metrics.Models;

#nullable enable

namespace Piipan.Metrics.Tests
{
    namespace Piipan.Metrics.Api.Tests
    {
        public class GetParticipantUploadsTests
        {
            static HttpRequest EventMock()
            {
                var context = new DefaultHttpContext();
                return context.Request;
            }

            static Mock<ILogger> Logger()
            {
                return new Mock<ILogger>();
            }

            public class ResultsQueryTests
            {
                [Fact]
                public async void ResultsQuerySuccess() {
                    Environment.SetEnvironmentVariable("KeyVaultName", "foo");
                    var logger = Logger();
                    var factory = new Mock<DbProviderFactory>() { DefaultValue = DefaultValue.Mock };
                    var cmd = new Mock<DbCommand>() { DefaultValue = DefaultValue.Mock };
                    var queryString = "SELECT * from participant_uploads";
                    factory.Setup(f => f.CreateCommand()).Returns(cmd.Object);

                    var results = await GetParticipantUploads.ResultsQuery(
                        factory.Object,
                        queryString,
                        logger.Object
                    );

                    Assert.Equal(new List<ParticipantUpload>(), results);

                    //teardown
                    Environment.SetEnvironmentVariable("KeyVaultName", null);
                }

            }

            public class ResultsQueryStringTests
            {
                // result is like a sql SELECT statement
                [Fact]
                public void ReturnsSelectStatement()
                {
                    var requestQuery = new Mock<IQueryCollection>();
                    var result = GetParticipantUploads.ResultsQueryString(requestQuery.Object);
                    Assert.Matches("SELECT state, uploaded_at FROM participant_uploads", result);
                }
                // when state query param is present, result includes a WHERE statement
                [Fact]
                public void WhenStatePresent() {
                    var requestQuery = new Mock<IQueryCollection>();
                    requestQuery.SetupGet(q => q["state"]).Returns(new StringValues("ea"));
                    var result = GetParticipantUploads.ResultsQueryString(requestQuery.Object);
                    Assert.Matches(@"WHERE lower\(state\) LIKE '%ea%'", result);
                }

                // when state query param is not present, result includes a WHERE statement
                [Fact]
                public void WhenStateNotPresent()
                {
                    var requestQuery = new Mock<IQueryCollection>();
                    var result = GetParticipantUploads.ResultsQueryString(requestQuery.Object);
                    Assert.DoesNotMatch(@"WHERE lower\(state\) LIKE '%ea%'", result);
                }

                // when perPage param is present, result includes it as LIMIT
                [Fact]
                public void WhenPerPagePresent()
                {
                    var requestQuery = new Mock<IQueryCollection>();
                    requestQuery.SetupGet(q => q["perPage"]).Returns(new StringValues("10"));
                    var result = GetParticipantUploads.ResultsQueryString(requestQuery.Object);
                    Assert.Matches("LIMIT 10", result);
                }

                // when perPage param is > 1, result string has correct OFFSET
                [Fact]
                public void OffsetIsCorrect()
                {
                    var requestQuery = new Mock<IQueryCollection>();
                    requestQuery.SetupGet(q => q["page"]).Returns(new StringValues("3"));
                    requestQuery.SetupGet(q => q["perPage"]).Returns(new StringValues("10"));
                    var result = GetParticipantUploads.ResultsQueryString(requestQuery.Object);
                    Assert.Matches("OFFSET 20", result);
                }
            }

            public class StrToIntWithDefaultTests
            {
                // when passed a proper string version of a number, returns the number
                [Fact]
                public void ReturnsIntOfString()
                {
                    var result = GetParticipantUploads.StrToIntWithDefault("5", 2);
                    Assert.Equal(5, result);
                }
                // when passed:
                // 1. an improper string version of a number
                // 2. null
                // returns the default number provided
                [Theory]
                [InlineData("foo")]
                [InlineData(null)]
                public void ReturnsSthg(string? inline)
                {
                    var result = GetParticipantUploads.StrToIntWithDefault(inline, 2);
                    Assert.Equal(2, result);
                }
            }

            public class NextPageParamsTests
            {
                // when there's a next page to be had, result is not null
                [Fact]
                public void WhenNextPage()
                {
                    var page = 1;
                    var perPage = 5;
                    var total = 6;
                    var result = GetParticipantUploads.NextPageParams(
                        null,
                        page,
                        perPage,
                        total);
                    Assert.Matches(@"\?page=2\&perPage=5", result);
                }
                // when there's no next page, result is null
                [Fact]
                public void WhenNotNextPage()
                {
                    var page = 2;
                    var perPage = 5;
                    var total = 6;
                    var result = GetParticipantUploads.NextPageParams(
                        null,
                        page,
                        perPage,
                        total);
                    Assert.Equal(null, result);
                }
                // when state is passed, result includes state param
                [Fact]
                public void WhenQueryPresent()
                {
                    var state = "ea";
                    var page = 1;
                    var perPage = 5;
                    var total = 6;
                    var result = GetParticipantUploads.NextPageParams(
                        state,
                        page,
                        perPage,
                        total);
                    Assert.Matches(@"\?state=ea", result);
                }

            }


            public class PrevPageParamsTests
            {
                // when there's a previous page to be had
                [Fact]
                public void WhenPrevPage()
                {
                    var page = 2;
                    var perPage = 5;
                    var total = 6;
                    var result = GetParticipantUploads.PrevPageParams(
                        null,
                        page,
                        perPage,
                        total);
                    Assert.Matches(@"\?page=1\&perPage=5", result);
                }
                // when there's not a previous page to be had
                [Fact]
                public void WhenNotPrevPage()
                {
                    var page = 1;
                    var perPage = 5;
                    var total = 6;
                    var result = GetParticipantUploads.PrevPageParams(
                        null,
                        page,
                        perPage,
                        total);
                    Assert.Equal(null, result);
                }
                // when state is passed, result includes state param
                [Fact]
                public void WhenQueryPresent()
                {
                    var state = "ea";
                    var page = 2;
                    var perPage = 5;
                    var total = 6;
                    var result = GetParticipantUploads.PrevPageParams(
                        state,
                        page,
                        perPage,
                        total);
                    Assert.Matches(@"\?state=ea", result);
                }

            }
            public class TotalQueryTests
            {
                [Fact]
                public async void ReturnsInt64()
                {
                    Environment.SetEnvironmentVariable("KeyVaultName", "foo");
                    var req = EventMock();
                    var logger = Logger();
                    var factory = new Mock<DbProviderFactory>() { DefaultValue = DefaultValue.Mock };
                    var cmd = new Mock<DbCommand>() { DefaultValue = DefaultValue.Mock };
                    factory.Setup(f => f.CreateCommand()).Returns(cmd.Object);

                    // Mocks foreign key used in participants table
                    cmd.Setup(c => c.ExecuteScalar()).Returns((Int64)5);

                    var result = await GetParticipantUploads.TotalQuery(
                        req,
                        factory.Object,
                        logger.Object
                    );

                    Assert.Equal(5, result);
                    Assert.IsType<Int64>(result);
                    // teardown
                    Environment.SetEnvironmentVariable("KeyVaultName", null);
                }
            }
        }
    }
}
