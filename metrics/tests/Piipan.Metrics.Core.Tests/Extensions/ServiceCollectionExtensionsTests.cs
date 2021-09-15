using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Extensions;
using Moq;
using Xunit;

namespace Piipan.Metrics.Core.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void RegisterCoreServices_AllResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<IDbConnection>(c => Mock.Of<IDbConnection>());

            // Act
            services.RegisterCoreServices();
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IParticipantUploadDao>());
            Assert.NotNull(provider.GetService<IParticipantUploadApi>());
        }
    }
}