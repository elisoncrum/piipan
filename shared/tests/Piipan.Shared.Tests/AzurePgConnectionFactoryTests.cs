using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Npgsql;
using Xunit;
using Moq;

namespace Piipan.Shared.Tests
{
    public class AzurePgConnectionFactoryTests
    {
        private string _connectionString;

        private void SetDatabaseConnectionString()
        {
            Environment.SetEnvironmentVariable(
                AzurePgConnectionFactory.DatabaseConnectionString,
                "Server=statedb;Database=ea;Port=5432;User Id=postgres;Password={password};"
            );
        }

        private Mock<AzureServiceTokenProvider> MockTokenProvider(string token = "token")
        {
            var mockProvider = new Mock<AzureServiceTokenProvider>(() =>
                new AzureServiceTokenProvider(null, "https://tts.test")
            );

            mockProvider
                .Setup(m => m.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            return mockProvider;
        }

        private Mock<DbProviderFactory> MockDbProviderFactory()
        {
            var connection = new Mock<DbConnection>();
            connection
                .SetupSet(m => m.ConnectionString = It.IsAny<string>())
                .Callback<string>(v => _connectionString = v);

            var factory = new Mock<DbProviderFactory>();
            factory
                .Setup(m => m.CreateConnection())
                .Returns(connection.Object);

            return factory;
        }

        [Fact]
        public async void Build_NoDatabaseConnectionString()
        {
            // Arrange
            var tokenProvider = MockTokenProvider().Object;
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory(tokenProvider, npgsqlFactory);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => factory.Build());
        }

        [Fact]
        public async void Build_MalformedDatabaseConnectionString()
        {
            // Arrange
            var tokenProvider = MockTokenProvider().Object;
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory(tokenProvider, npgsqlFactory);
            Environment.SetEnvironmentVariable(AzurePgConnectionFactory.DatabaseConnectionString, "not a connection string");

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => factory.Build());
        }

        [Fact]
        public async void Build_DefaultsToCommercialCloud()
        {
            // Arrange
            var tokenProvider = MockTokenProvider();
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory(tokenProvider.Object, npgsqlFactory);
            SetDatabaseConnectionString();

            // Act
            await factory.Build();

            // Assert
            tokenProvider.Verify(m => 
                m.GetAccessTokenAsync(AzurePgConnectionFactory.CommercialId, null, default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async void Build_UsesGovtCloudWhenSet()
        {
            // Arrange
            var tokenProvider = MockTokenProvider();
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory(tokenProvider.Object, npgsqlFactory);
            SetDatabaseConnectionString();
            Environment.SetEnvironmentVariable(
                AzurePgConnectionFactory.CloudName,
                AzurePgConnectionFactory.GovernmentCloud
            );

            // Act
            await factory.Build();

            // Assert
            tokenProvider.Verify(m => 
                m.GetAccessTokenAsync(AzurePgConnectionFactory.GovermentId, null, default(CancellationToken)), Times.Once);

            // Tear down
            Environment.SetEnvironmentVariable(AzurePgConnectionFactory.CloudName, null);
        }

        [Fact]
        public async Task Build_UsesTokenAsPassword()
        {
            // Arrange
            var expectedPassword = Guid.NewGuid().ToString();
            var tokenProvider = MockTokenProvider(expectedPassword);
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory(tokenProvider.Object, npgsqlFactory);
            var databaseName = Guid.NewGuid().ToString();
            SetDatabaseConnectionString();
            
            // Act
            var connection = await factory.Build(databaseName);

            // Assert
            Assert.Contains($"Password={expectedPassword}", _connectionString);
        }

        [Fact]
        public async Task Build_UsesDatabaseOverride()
        {
            // Arrange
            var tokenProvider = MockTokenProvider();
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory(tokenProvider.Object, npgsqlFactory);
            var databaseName = Guid.NewGuid().ToString();
            SetDatabaseConnectionString();
            
            // Act
            var connection = await factory.Build(databaseName);

            // Assert
            Assert.Contains($"Database={databaseName}", _connectionString);
        }
    }
}