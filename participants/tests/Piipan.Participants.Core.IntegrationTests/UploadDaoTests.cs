using System;
using Piipan.Participants.Core.DataAccessObjects;
using Dapper;
using Npgsql;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class UploadDaoTests : DbFixture
    {
        [Fact]
        public async void GetLatestUpload()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                InsertUpload();

                var expected = GetLastUploadId();
            
                var dao = new UploadDao(conn);

                // Act
                var result = await dao.GetLatestUpload();

                // Assert
                Assert.Equal(expected, result.Id);
            }
        }

        [Fact]
        public async void GetLatestUpload_ThrowsIfNone()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                ClearUploads();
            
                var dao = new UploadDao(conn);

                // Act / Assert
                await Assert.ThrowsAsync<InvalidOperationException>(() => dao.GetLatestUpload());
            }
        }

        [Fact]
        public async void AddUpload()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
            
                var dao = new UploadDao(conn);

                // Act
                var result = await dao.AddUpload();

                // Assert
                Assert.Equal(GetLastUploadId(), result.Id);
            }
        }
    }
}