using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class StartupTests
    {
        [Fact]
        public void TestClaimsProvider()
        {
            // Arrange
            var env = new Mock<IWebHostEnvironment>();
            var target = new Startup(MockConfiguration(), env.Object);
            var services = new ServiceCollection();

            // Act
            target.ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IClaimsProvider>());
        }

        public static IConfiguration MockConfiguration()
        {
            var section = new Mock<IConfigurationSection>();
            section
                .Setup(m => m["Email"])
                .Returns("email_claim_type");

            var config = new Mock<IConfiguration>();
            config
                .Setup(m => m.GetSection("Claims"))
                .Returns(section.Object);

            return config.Object;
        }
    }
}