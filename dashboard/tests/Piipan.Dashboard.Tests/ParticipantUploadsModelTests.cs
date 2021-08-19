using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Dashboard.Api;
using Piipan.Dashboard.Pages;
using Piipan.Shared.Claims;
using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class ParticipantUploadsModelTests
    {
        [Fact]
        public void BeforeOnGetAsync_TitleIsCorrect()
        {
            var mockApi = new Mock<IParticipantUploadRequest>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockRequestUrlProvider = new Mock<RequestUrlProvider>().Object;
            var pageModel = new ParticipantUploadsModel(
                mockApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                mockClaimsProvider,
                mockRequestUrlProvider
            );
            Assert.Equal("Participant Uploads", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public void BeforeOnGetAsync_PerPageDefaultIsCorrect()
        {
            Assert.True(ParticipantUploadsModel.PerPageDefault > 0, "page default is greater than 0");
        }

        [Fact]
        public void BeforeOnGetAsync_ApiUrlKeyIsCorrect()
        {
            Assert.IsType<String>(ParticipantUploadsModel.ApiUrlKey);
        }

        [Fact]
        public void BeforeOnGetAsync_BaseUrlIsCorrect()
        {
            Environment.SetEnvironmentVariable(ParticipantUploadsModel.ApiUrlKey, "http://example.com");
            var mockApi = new Mock<IParticipantUploadRequest>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockRequestUrlProvider = new Mock<RequestUrlProvider>().Object;
            var pageModel = new ParticipantUploadsModel(
                mockApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                mockClaimsProvider,
                mockRequestUrlProvider
            );
            Assert.Matches("http://example.com", pageModel.MetricsApiBaseUrl);
            Environment.SetEnvironmentVariable(ParticipantUploadsModel.ApiUrlKey, null);
        }

        [Fact]
        public void BeforeOnGetAsync_initializesParticipantUploadResults()
        {
            var mockApi = new Mock<IParticipantUploadRequest>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockRequestUrlProvider = new Mock<RequestUrlProvider>().Object;
            var pageModel = new ParticipantUploadsModel(
                mockApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                mockClaimsProvider,
                mockRequestUrlProvider
            );
            Assert.IsType<List<ParticipantUpload>>(pageModel.ParticipantUploadResults);
        }

        // sets participant uploads after Get request
        [Fact]
        public async void AfterOnGetAsync_setsParticipantUploadResults()
        {
            // setup env
            Environment.SetEnvironmentVariable(ParticipantUploadsModel.ApiUrlKey, "http://example.com");
            // setup mocks
            var participantUpload = new ParticipantUpload("eb", new DateTime());
            var data = new List<ParticipantUpload>();
            data.Add(participantUpload);
            var meta = new ParticipantUploadResponseMeta();
            var mockApi = mockApiWithResponse(data, meta);
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockRequestUrlProvider = new Mock<RequestUrlProvider>().Object;
            var pageContext = MockPageContext(new DefaultHttpContext());
            // setup page model with mocks
            var pageModel = new ParticipantUploadsModel(
                mockApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                mockClaimsProvider,
                mockRequestUrlProvider
            )
            {
                PageContext = pageContext
            };
            // run
            await pageModel.OnGetAsync();
            // assert
            Assert.Equal(participantUpload, pageModel.ParticipantUploadResults[0]);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            // teardown
            Environment.SetEnvironmentVariable(ParticipantUploadsModel.ApiUrlKey, null);
        }
        // sets participant uploads after Post request
        [Fact]
        public async void AfterOnPostAsync_setsParticipantUploadResults()
        {
            // setup env
            Environment.SetEnvironmentVariable(ParticipantUploadsModel.ApiUrlKey, "http://example.com");
            // setup mock api response
            var participantUpload = new ParticipantUpload("eb", new DateTime());
            var data = new List<ParticipantUpload>();
            data.Add(participantUpload);
            var meta = new ParticipantUploadResponseMeta();
            var mockApi = mockApiWithResponse(data, meta);
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockRequestUrlProvider = new Mock<RequestUrlProvider>().Object;
            // setup mock page context with form data
            var httpContext = new DefaultHttpContext();
            var form = new FormCollection(new Dictionary<string,
            Microsoft.Extensions.Primitives.StringValues>
            {
                { "state", "foo" }
            });
            httpContext.Request.Form = form;
            var pageContext = MockPageContext(httpContext);
            // setup page model with mocks
            var pageModel = new ParticipantUploadsModel(
                mockApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                mockClaimsProvider,
                mockRequestUrlProvider
            )
            {
                PageContext = pageContext
            };
            // run
            await pageModel.OnPostAsync();
            // assert
            Assert.Equal(participantUpload, pageModel.ParticipantUploadResults[0]);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            // teardown
            Environment.SetEnvironmentVariable(ParticipantUploadsModel.ApiUrlKey, null);
        }

        private Mock<IParticipantUploadRequest> mockApiWithResponse(
            List<ParticipantUpload> data,
            ParticipantUploadResponseMeta meta
        )
        {
            var mockResponse = new ParticipantUploadResponse();
            mockResponse.meta = new ParticipantUploadResponseMeta();
            mockResponse.data = data;
            var mockApi = new Mock<IParticipantUploadRequest>();
            mockApi.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult(mockResponse));
            return mockApi;
        }
        // setup mock httpcontext for page model,
        // which provides the Route object to the model
        private PageContext MockPageContext(DefaultHttpContext httpContext)
        {
            var modelState = new ModelStateDictionary();
            var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            var pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };
            return pageContext;
        }

        private IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }
    }
}
