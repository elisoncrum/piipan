using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Piipan.Participants.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Database;
using Xunit;

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
            services.AddTransient<IDbConnectionFactory<ParticipantsDb>>(c => Mock.Of<IDbConnectionFactory<ParticipantsDb>>());

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
