using System;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Client.Extensions;
using Xunit;

namespace Piipan.Match.Client.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void RegisterMatchClientServices_DevelopmentServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;
            Environment.SetEnvironmentVariable("OrchApiUri", "https://tts.test");

            // Act
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchApi>());
            Assert.IsType<AzureCliCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchClientServices_StagingServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Staging;
            Environment.SetEnvironmentVariable("OrchApiUri", "https://tts.test");

            // Act
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchClientServices_ProductionServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Production;
            Environment.SetEnvironmentVariable("OrchApiUri", "https://tts.test");

            // Act
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IMatchApi>());
            Assert.IsType<ManagedIdentityCredential>(provider.GetService<TokenCredential>());
        }

        [Fact]
        public void RegisterMatchClientServices_HttpClientBaseAddressSet()
        {
            // Arrange
            var services = new ServiceCollection();
            var env = Mock.Of<IHostEnvironment>();
            env.EnvironmentName = Environments.Development;
            Environment.SetEnvironmentVariable("OrchApiUrl", "https://tts.test");

            // Act
            services.RegisterMatchClientServices(env);
            var provider = services.BuildServiceProvider();

            var clientFactory = provider.GetService<IHttpClientFactory>();
            var client = clientFactory.CreateClient("MatchClient");
            Assert.Equal("https://tts.test/", client.BaseAddress.ToString());
        }
    }
}