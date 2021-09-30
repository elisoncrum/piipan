using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Resolvers;
using Piipan.Shared;
using Piipan.Shared.Authentication;
using FluentValidation;
using Moq;
using Xunit;

namespace Piipan.Match.Func.Api.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new Mock<IFunctionsHostBuilder>();
            builder
                .Setup(m => m.Services)
                .Returns(services);

            var target = new Startup();

            // Act
            target.Configure(builder.Object);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchResolver>());
            Assert.NotNull(provider.GetService<IValidator<OrchMatchRequest>>());
            Assert.NotNull(provider.GetService<IValidator<RequestPerson>>());
            Assert.NotNull(provider.GetService<IStreamParser<OrchMatchRequest>>());
            Assert.NotNull(provider.GetService<ITokenProvider>());
            Assert.NotNull(provider.GetService<IDbConnectionFactory>());
        }
    }
}