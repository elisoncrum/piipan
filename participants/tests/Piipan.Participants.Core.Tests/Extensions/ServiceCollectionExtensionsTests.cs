using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Participants.Core.Extensions;
using Moq;
using Xunit;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Api;

namespace Piipan.Participants.Core.Tests.Extensions
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
            services.RegisterParticipantsServices();
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IParticipantDao>());
            Assert.NotNull(provider.GetService<IUploadDao>());
            Assert.NotNull(provider.GetService<IParticipantApi>());
        }
    }
}