using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Authentication;
using Piipan.Shared.Claims;
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
                    It.Is<Uri>(u => u.ToString().Contains("/query")),
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

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange

            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockApiClient,
                mockClaimsProvider
                );
            // act
            // assert
            Assert.Equal("", pageModel.Title);
            Assert.Equal("", pageModel.Email);
        }
        [Fact]
        public void TestAfterOnGet()
        {
            // arrange
            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockApiClient, mockClaimsProvider);

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async void MatchSetsResults()
        {
            // arrange
            var returnValue = @"{
                ""lookup_id"": ""BBB2222"",
                ""matches"": [{
                    ""first"": ""Theodore"",
                    ""middle"": ""Carri"",
                    ""last"": ""Farrington"",
                    ""ssn"": ""000-00-0000"",
                    ""dob"": ""2021-01-01"",
                    ""state"": ""ea"",
                    ""state_abbr"": ""ea""
                }]
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
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockClient, mockClaimsProvider);
            pageModel.Query = requestPii;

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<MatchResponse>(pageModel.QueryResult);
            Assert.NotNull(pageModel.QueryResult);
            Assert.NotNull(pageModel.QueryResult.matches);
            Assert.False(pageModel.NoResults);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async void MatchNoResults()
        {
            // arrange
            var returnValue = @"{
                ""lookup_id"": null,
                ""matches"": []
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
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockClient, mockClaimsProvider);
            pageModel.Query = requestPii;

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<MatchResponse>(pageModel.QueryResult);
            Assert.Null(pageModel.QueryResult.lookupId);
            Assert.Empty(pageModel.QueryResult.matches);
            Assert.True(pageModel.NoResults);
            Assert.Equal("noreply@tts.test", pageModel.Email);
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
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockClient, mockClaimsProvider);
            pageModel.Query = requestPii;

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.RequestError);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }
    }
}
