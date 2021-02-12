using System;
using Xunit;
using Piipan.Metrics.Api.Serializers;

namespace Piipan.Metrics.Tests
{
    namespace Piipan.Metrics.Api.Tests
    {
        public class MetaTests
        {
            [Fact]
            public static void HasAPage()
            {
                var meta = new Meta();
                meta.page = 1;
                Assert.Equal(1, meta.page);
            }
            [Fact]
            public static void HasAPerPage()
            {
                var meta = new Meta();
                meta.perPage = 2;
                Assert.Equal(2, meta.perPage);
            }

            [Fact]
            public static void HasATotal()
            {
                var meta = new Meta();
                meta.total = 5;
                Assert.Equal(5, meta.total);
            }
        }
    }
}
