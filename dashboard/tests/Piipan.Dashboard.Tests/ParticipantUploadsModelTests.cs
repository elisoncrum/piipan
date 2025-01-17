using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Dashboard.Pages;
using Piipan.Metrics.Api;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class ParticipantUploadsModelTests
    {
        [Fact]
        public void BeforeOnGetAsync_TitleIsCorrect()
        {
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new ParticipantUploadsModel(
                Mock.Of<IParticipantUploadReaderApi>(),
                new NullLogger<ParticipantUploadsModel>(),
                mockClaimsProvider
            );
            pageModel.PageContext.HttpContext = contextMock().Object;

            Assert.Equal("Most recent upload from each state", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public void BeforeOnGetAsync_PerPageDefaultIsCorrect()
        {
            Assert.True(ParticipantUploadsModel.PerPageDefault > 0, "page default is greater than 0");
        }

        [Fact]
        public void BeforeOnGetAsync_InitializesParticipantUploadResults()
        {
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new ParticipantUploadsModel(
                Mock.Of<IParticipantUploadReaderApi>(),
                new NullLogger<ParticipantUploadsModel>(),
                mockClaimsProvider
            );
            Assert.IsType<List<ParticipantUpload>>(pageModel.ParticipantUploadResults);
        }

        // sets participant uploads after Get request
        [Fact]
        public async void AfterOnGetAsync_SetsParticipantUploadResults()
        {
            // Arrange
            var response = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload { State = "ea", UploadedAt = DateTime.Now }
                },
                Meta = new Meta()
            };
            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetLatestUploadsByState())
                .ReturnsAsync(response);

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                claimsProviderMock("noreply@tts.test")
            );

            var httpContext = contextMock().Object;
            pageModel.PageContext.HttpContext = httpContext;

            // Act
            await pageModel.OnGetAsync();

            // assert
            Assert.Equal(response.Data.First(), pageModel.ParticipantUploadResults[0]);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async Task AfterOnGetAsync_ApiThrows()
        {
            // Arrange
            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetLatestUploadsByState())
                .ThrowsAsync(new Exception("api broke"));

            var logger = new Mock<ILogger<ParticipantUploadsModel>>();

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                logger.Object,
                claimsProviderMock("noreply@tts.test")
            );

            // Act
            await pageModel.OnGetAsync();

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("api broke")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task AfterOnGetAsync_ApiThrowsHttpRequestExecption()
        {
            // Arrange
            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetLatestUploadsByState())
                .ThrowsAsync(new HttpRequestException("api broke"));

            var logger = new Mock<ILogger<ParticipantUploadsModel>>();

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                logger.Object,
                claimsProviderMock("noreply@tts.test")
            );

            // Act
            await pageModel.OnGetAsync();

            // Assert
            Assert.Contains("You may be able to try again", pageModel.RequestError);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("api broke")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        // sets participant uploads after Post request
        [Fact]
        public async void AfterOnPostAsync_setsParticipantUploadResults()
        {
            // Arrange
            var response = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload { State = "eb", UploadedAt = DateTime.Now }
                },
                Meta = new Meta()
            };
            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetUploads("eb", ParticipantUploadsModel.PerPageDefault, 1))
                .ReturnsAsync(response);

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                claimsProviderMock("noreply@tts.test")
            );

            var request = requestMock();
            request
                .Setup(m => m.Form)
                .Returns(new FormCollection(new Dictionary<string, StringValues>
                {
                    { "state", "eb" }
                }));

            var httpContext = contextMock();
            httpContext
                .Setup(m => m.Request)
                .Returns(request.Object);
            pageModel.PageContext.HttpContext = httpContext.Object;

            // Act
            await pageModel.OnPostAsync();

            // Assert
            Assert.Equal(response.Data.First(), pageModel.ParticipantUploadResults[0]);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async Task AfterOnPostAsync_ApiThrows()
        {
            // Arrange
            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetUploads("eb", ParticipantUploadsModel.PerPageDefault, 1))
                .ThrowsAsync(new Exception("api broke"));

            var logger = new Mock<ILogger<ParticipantUploadsModel>>();

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                logger.Object,
                claimsProviderMock("noreply@tts.test")
            );

            var request = requestMock();
            request
                .Setup(m => m.Form)
                .Returns(new FormCollection(new Dictionary<string, StringValues>
                {
                    { "state", "eb" }
                }));

            var httpContext = contextMock();
            httpContext
                .Setup(m => m.Request)
                .Returns(request.Object);
            pageModel.PageContext.HttpContext = httpContext.Object;

            // Act
            await pageModel.OnPostAsync();

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("api broke")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task AfterOnPostAsync_ApiThrowsHttpRequestException()
        {
            // Arrange
            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetUploads("eb", ParticipantUploadsModel.PerPageDefault, 1))
                .ThrowsAsync(new HttpRequestException("api broke"));

            var logger = new Mock<ILogger<ParticipantUploadsModel>>();

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                logger.Object,
                claimsProviderMock("noreply@tts.test")
            );

            var request = requestMock();
            request
                .Setup(m => m.Form)
                .Returns(new FormCollection(new Dictionary<string, StringValues>
                {
                    { "state", "eb" }
                }));

            var httpContext = contextMock();
            httpContext
                .Setup(m => m.Request)
                .Returns(request.Object);
            pageModel.PageContext.HttpContext = httpContext.Object;

            // Act
            await pageModel.OnPostAsync();

            // Assert
            Assert.Contains("You may be able to try again", pageModel.RequestError);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("api broke")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        private IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }

        public static Mock<HttpRequest> requestMock()
        {
            var request = new Mock<HttpRequest>();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            return request;
        }

        public static Mock<HttpContext> contextMock()
        {
            var request = requestMock();

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);

            return context;
        }
    }
}
