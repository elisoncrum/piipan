using Piipan.Metrics.Api;
using Piipan.Metrics.Func.Api.Builders;
using Xunit;
using Moq;

namespace Piipan.Metrics.Func.Api.Tests.Builders
{
    public class MetaBuilderTests
    {
        [Theory]
        [InlineData(0, 0)]
        public void Build_Default_Empty(int uploadCount, int expectedTotal)
        {
            // Arrange
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(uploadCount);

            var builder = new MetaBuilder(uploadApi.Object);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(expectedTotal, meta.total);
            Assert.Equal(0, meta.page);
            Assert.Equal(0, meta.perPage);
            Assert.Empty(meta.nextPage);
            Assert.Null(meta.prevPage);
        }

        [Theory]
        [InlineData(1, 1)]
        public void Build_Default_NonEmpty(int uploadCount, int expectedTotal)
        {
            // Arrange
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(uploadCount);

            var builder = new MetaBuilder(uploadApi.Object);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(expectedTotal, meta.total);
            Assert.Equal(0, meta.page);
            Assert.Equal(0, meta.perPage);
            Assert.Contains("page=1", meta.nextPage);
            Assert.Null(meta.prevPage);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        public void Build_FirstPage(int page, int expectedPage)
        {
            // Arrange
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(5);

            var builder = new MetaBuilder(uploadApi.Object);
            builder.SetPage(page);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(5, meta.total);
            Assert.Equal(expectedPage, meta.page);
            Assert.Equal(0, meta.perPage);
            Assert.Contains($"page={expectedPage + 1}", meta.nextPage);
            Assert.Null(meta.prevPage);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(4, 4)]
        public void Build_NotFirstPage(int page, int expectedPage)
        {
            // Arrange
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(5);

            var builder = new MetaBuilder(uploadApi.Object);
            builder.SetPage(page);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(5, meta.total);
            Assert.Equal(expectedPage, meta.page);
            Assert.Equal(0, meta.perPage);
            Assert.Contains($"page={expectedPage + 1}", meta.nextPage);
            Assert.Contains($"page={expectedPage - 1}", meta.prevPage);
        }

        [Theory]
        [InlineData(2, 2, 4, 2, 2, 4)]
        [InlineData(4, 4, 15, 4, 4, 15)]
        public void Build_LastPage(
            int page, 
            int perPage, 
            int uploadCount, 
            int expectedPage, 
            int expectedPerPage,
            int expectedTotal)
        {
            // Arrange
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(uploadCount);

            var builder = new MetaBuilder(uploadApi.Object);
            builder.SetPage(page);
            builder.SetPerPage(perPage);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(expectedTotal, meta.total);
            Assert.Equal(expectedPage, meta.page);
            Assert.Equal(expectedPerPage, meta.perPage);
            Assert.Empty(meta.nextPage);
            Assert.Contains($"page={expectedPage - 1}", meta.prevPage);
            Assert.Contains($"perPage={expectedPerPage}", meta.prevPage);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(4, 4)]
        public void Build_WithPerPage(int perPage, int expectedPerPage)
        {
            // Arrange
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(5);

            var builder = new MetaBuilder(uploadApi.Object);
            builder.SetPerPage(perPage);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(5, meta.total);
            Assert.Equal(0, meta.page);
            Assert.Equal(expectedPerPage, meta.perPage);
            Assert.Contains($"perPage={expectedPerPage}",meta.nextPage);
            Assert.Null(meta.prevPage);
        }

        [Theory]
        [InlineData("ea", "ea")]
        [InlineData("eb", "eb")]
        [InlineData("somethinglonger", "somethinglonger")]
        public void Build_WithState(string state, string expectedState)
        {
            // Arrange
            var uploadApi = new Mock<IParticipantUploadApi>();
            uploadApi
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(5);

            var builder = new MetaBuilder(uploadApi.Object);
            builder.SetPage(2);
            builder.SetState(state);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(5, meta.total);
            Assert.Equal(2, meta.page);
            Assert.Contains($"state={expectedState}", meta.nextPage);
            Assert.Contains($"state={expectedState}", meta.prevPage);
        }
    }
}