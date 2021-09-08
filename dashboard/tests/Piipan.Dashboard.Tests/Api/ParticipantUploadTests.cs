using System;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class ParticipantUploadTests
    {
        [Fact]
        public void FormattedUploadAt_ReturnsResult()
        {
            const string state = "ea";
            DateTime uploaded_at = new DateTime(1970, 1, 1, 0, 0, 0);

            var upload = new ParticipantUpload(state, uploaded_at);

            Assert.IsType<string>(upload.FormattedUploadedAt());
            Assert.NotEmpty(upload.FormattedUploadedAt());
        }

        // Tests the method only
        // Formatting is tested in `shared/tests`
        [Fact]
        public void RelativeUploadAt_ReturnsResult()
        {
            const string state = "ea";
            DateTime uploaded_at = new DateTime(1970, 1, 1, 0, 0, 0);

            var upload = new ParticipantUpload(state, uploaded_at);

            Assert.IsType<string>(upload.RelativeUploadedAt());
            Assert.NotEmpty(upload.RelativeUploadedAt());
        }
    }
}
