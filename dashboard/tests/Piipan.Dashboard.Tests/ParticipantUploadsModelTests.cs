using System;
using System.Collections.Generic;
using Xunit;
using Piipan.Dashboard.Pages;

namespace Piipan.Dashboard.Tests
{
    public class ParticipantUploadsModelTests
    {
        [Fact]
        public void BeforeOnGetAsync_TitleIsCorrect()
        {
            var pageModel = new ParticipantUploadsModel();
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
            Assert.Matches("http", ParticipantUploadsModel.BaseUrl);
        }
    }
}
