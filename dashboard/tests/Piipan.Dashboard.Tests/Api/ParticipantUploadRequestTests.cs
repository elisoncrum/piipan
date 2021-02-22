using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Piipan.Dashboard.Api;


namespace Piipan.Dashboard.Tests
{
    public class ParticipantUploadRequestTests
    {
        static Mock<HttpMessageHandler> MessageHandlerMock(string mockResponse)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockResponse),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(resp);

            return handlerMock;
        }

        [Fact]
        public async void GetSuccess()
        {
            var mockResponse = @"{
                ""meta"": {
                    ""page"": 1,
                    ""perPage"": 5,
                    ""total"": 5,
                    ""nextPage"": null,
                    ""prevPage"": null
                },
                ""data"": [
                    {
                        ""state"": ""eb"",
                        ""uploaded_at"": ""1/1/0001 12:00:00 AM""
                    }
                ]
            }";
            var handlerMock = MessageHandlerMock(mockResponse);
            var httpClient = new HttpClient(handlerMock.Object);
            var api = new ParticipantUploadRequest(httpClient);
            var resultResponse = await api.Get("http://example.com");

            Assert.NotNull(resultResponse);
            Assert.Equal(1, resultResponse.meta.page);
            Assert.IsType<ParticipantUpload>(resultResponse.data[0]);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
               ItExpr.IsAny<CancellationToken>());
        }
    }
    public class ParticipantUploadResponseMetaTests
    {
        [Fact]
        public void CreateSuccess()
        {
            var obj = new ParticipantUploadResponseMeta()
            {
                page = 1,
                perPage = 5,
                total = 10,
                prevPage = null,
                nextPage = "foobar"
            };
            Assert.Equal(1, obj.page);
            Assert.Equal(5, obj.perPage);
            Assert.Equal(10, obj.total);
            Assert.Null(obj.prevPage);
            Assert.Equal("foobar", obj.nextPage);
        }
    }

    public class ParticipantUploadResponseTests
    {
        [Fact]
        public void CreateSuccess()
        {
            var meta = new ParticipantUploadResponseMeta()
            {
                page = 1,
                perPage = 5,
                total = 10,
                prevPage = null,
                nextPage = "foobar"
            };
            var record = new ParticipantUpload("eb", new DateTime());
            var list = new List<ParticipantUpload>() { record };
            var response = new ParticipantUploadResponse()
            {
                meta = meta,
                data = list
            };
            Assert.Equal(meta, response.meta);
            Assert.Equal(list[0], response.data[0]);
        }
    }
}
