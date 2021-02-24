using System;
using System.Collections.Generic;
using Piipan.Dashboard.Pages;
using Piipan.Dashboard.Api;
using Moq;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class ParticipantUploadsModelTests
    {
        [Fact]
        public void BeforeOnGetAsync_TitleIsCorrect()
        {
            var mockApi = new Mock<IParticipantUploadRequest>();
            var pageModel = new ParticipantUploadsModel(mockApi.Object);
            Assert.Equal("Participant Uploads", pageModel.Title);
        }

        [Fact]
        public void BeforeOnGetAsync_PerPageDefaultIsCorrect()
        {
            Assert.True(ParticipantUploadsModel.PerPageDefault > 0, "page default is greater than 0");
        }

        [Fact]
        public void BeforeOnGetAsync_BaseUrlIsCorrect()
        {
            Environment.SetEnvironmentVariable("MetricsApiUri", "http://example.com");
            var mockApi = new Mock<IParticipantUploadRequest>();
            var pageModel = new ParticipantUploadsModel(mockApi.Object);
            Assert.Matches("http://example.com", pageModel.BaseUrl);
            Environment.SetEnvironmentVariable("MetricsApiUri", null);
        }

        [Fact]
        public void BeforeOnGetAsync_initializesParticipantUploadResults()
        {
            var mockApi = new Mock<IParticipantUploadRequest>();
            var pageModel = new ParticipantUploadsModel(mockApi.Object);
            Assert.IsType<List<ParticipantUpload>>(pageModel.ParticipantUploadResults);
        }

        // Add more here
    }
}
