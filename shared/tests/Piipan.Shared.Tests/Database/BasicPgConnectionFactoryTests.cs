using System;
using System.Data.Common;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Piipan.Shared.Database.Tests
{
    [Collection("Piipan.Shared.ConnectionFactories")]
    public class BasicPgConnectionFactoryTests
    {
        private const string ConnectionString = "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};";
        private struct MockType { };
        private string _connectionString;

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
            var npgsqlFactory = MockDbProviderFactory().Object;

            // Act / Assert
            Assert.Throws<ArgumentException>(() =>
                new BasicPgConnectionFactory<MockType>(
                    npgsqlFactory,
                    String.Empty)
            );
        }

        [Fact]
        public async void Build_MalformedDatabaseConnectionString()
        {
            // Arrange\
            var npgsqlFactory = MockDbProviderFactory().Object;
            var malformedString = "not a connection string";
            var factory = new BasicPgConnectionFactory<MockType>(npgsqlFactory, malformedString);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => factory.Build());
        }

        [Fact]
        public async Task Build_UsesDatabaseOverride()
        {
            // Arrange
            var npgsqlFactory = MockDbProviderFactory().Object;
            var factory = new BasicPgConnectionFactory<MockType>(npgsqlFactory, ConnectionString);
            var databaseName = Guid.NewGuid().ToString();

            // Act
            var connection = await factory.Build(databaseName);

            // Assert
            Assert.Contains($"Database={databaseName}", _connectionString);
        }
    }
}
