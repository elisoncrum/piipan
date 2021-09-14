using System;
using System.Data.Common;
using Xunit;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Tests
{
    namespace Piipan.Metrics.Models.Tests
    {
        public class ParticipantUploadTests
        {
            [Fact]
            public static void CreateSuccess()
            {
                var result = new ParticipantUpload();
                result.State = "foobar";
                result.UploadedAt = new DateTime();

                Assert.Equal("foobar", result.State);
                Assert.Equal(typeof(DateTime), result.UploadedAt.GetType());
            }
        }

    }
}
