using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Piipan.Shared.Tests
{
    [Collection("Piipan.Shared.ConnectionFactories")]
    public class BasicPgConnectionFactoryTests
    {
        private string _connectionString;

        private void SetDatabaseConnectionString()
        {
            Environment.SetEnvironmentVariable(
                BasicPgConnectionFactory.DatabaseConnectionString,
                "Server=statedb;Database=ea;Port=5432;User Id=postgres;Password={password};"
            );
        }

        private void ClearDatabaseConnectionString()
        {
            Environment.SetEnvironmentVariable(
                BasicPgConnectionFactory.DatabaseConnectionString,
                null
            );
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
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new BasicPgConnectionFactory(npgsqlFactory);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => factory.Build());
            
            // Tear down
            ClearDatabaseConnectionString();
        }

        [Fact]
        public async void Build_MalformedDatabaseConnectionString()
        {
            // Arrange\
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new BasicPgConnectionFactory(npgsqlFactory);
            Environment.SetEnvironmentVariable(AzurePgConnectionFactory.DatabaseConnectionString, "not a connection string");

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => factory.Build());

            // Tear down
            ClearDatabaseConnectionString();
        }

        [Fact]
        public async Task Build_UsesDatabaseOverride()
        {
            // Arrange
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new BasicPgConnectionFactory(npgsqlFactory);
            var databaseName = Guid.NewGuid().ToString();
            SetDatabaseConnectionString();
            
            // Act
            var connection = await factory.Build(databaseName);

            // Assert
            Assert.Contains($"Database={databaseName}", _connectionString);

            // Tear down
            ClearDatabaseConnectionString();
        }
    }
}