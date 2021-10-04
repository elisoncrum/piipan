using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Moq;
using Npgsql;
using Xunit;

namespace Piipan.Shared.Database.Tests
{
    [Collection("Piipan.Shared.ConnectionFactories")]
    public class AzurePgConnectionFactoryTests
    {
        private const string ConnectionString = "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};";
        private struct MockType { };
        private string _connectionString;

        private void ClearEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable(AzurePgConnectionFactory<MockType>.CloudName, null);
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
        public void Build_NoDatabaseConnectionString()
        {
            // Arrange
            var tokenProvider = MockTokenProvider().Object;
            var npgsqlFactory = MockDbProviderFactory().Object;

            // Act / Assert
            Assert.Throws<ArgumentException>(() =>
                new AzurePgConnectionFactory<MockType>(
                    tokenProvider, npgsqlFactory, String.Empty)
            );
        }

        [Fact]
        public async Task Build_MalformedDatabaseConnectionString()
        {
            // Arrange
            var tokenProvider = MockTokenProvider().Object;
            var npgsqlFactory = MockDbProviderFactory().Object;
            var malformedString = "not a connection string";
            var factory = new AzurePgConnectionFactory<MockType>(
                    tokenProvider, npgsqlFactory, malformedString);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => factory.Build());
        }

        [Fact]
        public async void Build_DefaultsToCommercialCloud()
        {
            // Arrange
            var tokenProvider = MockTokenProvider();
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory<MockType>(
                tokenProvider.Object, npgsqlFactory, ConnectionString);

            // Act
            await factory.Build();

            // Assert
            tokenProvider.Verify(m =>
                m.GetAccessTokenAsync(AzurePgConnectionFactory<MockType>.CommercialId, null, default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async void Build_UsesGovtCloudWhenSet()
        {
            // Arrange
            var tokenProvider = MockTokenProvider();
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory<MockType>(
                tokenProvider.Object, npgsqlFactory, ConnectionString);
            Environment.SetEnvironmentVariable(
                AzurePgConnectionFactory<MockType>.CloudName,
                AzurePgConnectionFactory<MockType>.GovernmentCloud
            );

            // Act
            await factory.Build();

            // Assert
            tokenProvider.Verify(m =>
                m.GetAccessTokenAsync(AzurePgConnectionFactory<MockType>.GovermentId, null, default(CancellationToken)), Times.Once);

            // Tear down
            ClearEnvironmentVariables();
        }

        [Fact]
        public async Task Build_UsesTokenAsPassword()
        {
            // Arrange
            var expectedPassword = Guid.NewGuid().ToString();
            var tokenProvider = MockTokenProvider(expectedPassword);
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new AzurePgConnectionFactory<MockType>(
                tokenProvider.Object, npgsqlFactory, ConnectionString);
            var databaseName = Guid.NewGuid().ToString();

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
            var factory = new AzurePgConnectionFactory<MockType>(
                tokenProvider.Object, npgsqlFactory, ConnectionString);
            var databaseName = Guid.NewGuid().ToString();

            // Act
            var connection = await factory.Build(databaseName);

            // Assert
            Assert.Contains($"Database={databaseName}", _connectionString);
        }
    }
}
