using System;
using System.Threading.Tasks;
using Xunit;
using Piipan.Metrics.Api;
using System.Collections.Generic;
using Moq;
using Piipan.Shared.Http;

namespace Piipan.Metrics.Client.Tests
{
    public class ParticipantUploadClientTests
    {
        [Fact]
        public async Task GetUploads_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload { State = "ea", UploadedAt = DateTime.Now }
                },
                Meta = new Meta
                {
                    Page = 1,
                    Total = 1
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<ParticipantUploadClient>>();
            apiClient
                .Setup(m => m.GetAsync<GetParticipantUploadsResponse>("GetParticipantUploads"))
                .ReturnsAsync(expectedResponse);

            var client = new ParticipantUploadClient(apiClient.Object);

            // Act
            var response = await client.GetUploads("ea", 1, 1);

            // Assert
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task GetLatestUploadsByState_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload { State = "ea", UploadedAt = DateTime.Now }
                },
                Meta = new Meta
                {
                    Page = 1,
                    Total = 1
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<ParticipantUploadClient>>();
            apiClient
                .Setup(m => m.GetAsync<GetParticipantUploadsResponse>("GetLastUpload"))
                .ReturnsAsync(expectedResponse);

            var client = new ParticipantUploadClient(apiClient.Object);

            // Act
            var response = await client.GetLatestUploadsByState();

            // Assert
            Assert.Equal(expectedResponse, response);
        }
    }
}
