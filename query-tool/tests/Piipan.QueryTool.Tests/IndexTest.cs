using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Authentication;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class IndexPageTests
    {
        public IndexPageTests()
        {
            Environment.SetEnvironmentVariable("OrchApiUri", "https://localhost/");
        }

        public static IAuthorizedApiClient clientMock(HttpStatusCode statusCode, string returnValue)
        {
            var clientMock = new Mock<IAuthorizedApiClient>();
            clientMock
                .Setup(c => c.PostAsync(
                    It.Is<Uri>(u => u.ToString().Contains("/find_matches")),
                    It.IsAny<StringContent>()
                ))
                .Returns(Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(returnValue)
                }));

            return clientMock.Object;
        }

        public static IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }

        public static HttpContext contextMock()
        {
            var request = new Mock<HttpRequest>();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);

            return context.Object;
        }

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange

            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockApiClient,
                mockClaimsProvider,
                mockLdsDeidentifier
                );
            // act
            // assert
            Assert.Equal("", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }
        [Fact]
        public void TestAfterOnGet()
        {
            // arrange
            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockApiClient,
                mockClaimsProvider,
                mockLdsDeidentifier
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async void MatchSetsResults()
        {
            // arrange
            var returnValue = @"{
                ""data"": {
                    ""results"": [
                        {
                            ""matches"": [{
                                ""lds_hash"": ""foobar"",
                                ""state"": ""ea"",
                                ""case_id"": ""caseId"",
                                ""participant_id"": ""pId"",
                                ""benefits_end_month"": ""2021-05"",
                                ""recent_benefit_months"": [
                                    ""2021-04"",
                                    ""2021-03"",
                                    ""2021-02""
                                ],
                                ""protect_location"": false
                            }]
                        }
                    ],
                    ""errors"": []
                }
            }";
            var requestPii = new PiiRecord
            {
                FirstName = "Theodore",
                LastName = "Farrington",
                SocialSecurityNum = "987-65-4320",
                DateOfBirth = new DateTime(1931, 10, 13)
            };
            var mockClient = clientMock(HttpStatusCode.OK, returnValue);
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClient,
                mockClaimsProvider,
                mockLdsDeidentifier
            );
            pageModel.Query = requestPii;
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<MatchResponse>(pageModel.QueryResult);
            Assert.NotNull(pageModel.QueryResult);
            Assert.NotNull(pageModel.QueryResult.Data.Results[0].Matches);
            Assert.False(pageModel.NoResults);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async void MatchNoResults()
        {
            // arrange
            var returnValue = @"{
                ""data"": {
                    ""results"": [
                        {
                            ""matches"": []
                        }
                    ],
                    ""errors"": []
                }
            }";
            var requestPii = new PiiRecord
            {
                FirstName = "Theodore",
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
            };
            var mockClient = clientMock(HttpStatusCode.OK, returnValue);
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClient,
                mockClaimsProvider,
                mockLdsDeidentifier
            );
            pageModel.Query = requestPii;
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<MatchResponse>(pageModel.QueryResult);
            Assert.Empty(pageModel.QueryResult.Data.Results[0].Matches);
            Assert.True(pageModel.NoResults);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async void MatchCapturesApiError()
        {
            // arrange
            var requestPii = new PiiRecord
            {
                FirstName = "Theodore",
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
            };
            var mockClient = clientMock(HttpStatusCode.BadRequest, "");
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClient,
                mockClaimsProvider,
                mockLdsDeidentifier
            );
            pageModel.Query = requestPii;
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.RequestError);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }
    }
}
