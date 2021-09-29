using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Match.Func.Api.Models;
using Piipan.Match.Func.Api.Parsers;
using Piipan.Match.Func.Api.Resolvers;
using Piipan.Match.Func.Api.Validators;
using Piipan.Participants.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Participants.Core.Services;
using Piipan.Shared;
using Piipan.Shared.Authentication;
using Xunit;
using FluentValidation;


namespace Piipan.Match.Func.Api.IntegrationTests
{
    public class ApiIntegrationTests : DbFixture
    {
        static Participant FullRecord()
        {
            return new Participant
            {
                // farrington,1931-10-13,000-12-3456
                LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458",
                CaseId = "CaseIdExample",
                ParticipantId = "ParticipantIdExample",
                BenefitsEndDate = new DateTime(1970, 1, 31),
                RecentBenefitMonths = new List<DateTime>() {
                  new DateTime(2021, 5, 31),
                  new DateTime(2021, 4, 30),
                  new DateTime(2021, 3, 31)
                },
                ProtectLocation = true
            };
        }

        static String JsonBody(object[] records)
        {
            var data = new
            {
                data = records
            };

            return JsonConvert.SerializeObject(data);
        }

        static Mock<HttpRequest> MockRequest(string jsonBody)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(jsonBody);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "From", "a user" },
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));

            return mockRequest;
        }

        static MatchApi Construct()
        {
            Environment.SetEnvironmentVariable("States", "ea");

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IValidator<OrchMatchRequest>, OrchMatchRequestValidator>();
            services.AddTransient<IValidator<RequestPerson>, RequestPersonValidator>();

            services.AddTransient<IStreamParser<OrchMatchRequest>, OrchMatchRequestParser>();

            services.AddTransient<IDbConnectionFactory>(s => 
            {
                return new BasicPgConnectionFactory(NpgsqlFactory.Instance);
            });
            services.RegisterParticipantsServices();

            services.AddTransient<IMatchResolver, MatchResolver>();

            var provider = services.BuildServiceProvider();  

            var api = new MatchApi(
                provider.GetService<IMatchResolver>(),
                provider.GetService<IStreamParser<OrchMatchRequest>>()
            );

            return api;
        }

        [Fact]
        public async void ApiReturnsMatches()
        {
            // Arrange
            var record = FullRecord();
            var logger = Mock.Of<ILogger>();
            var body = new object[] { record };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            Insert(record);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);

            var person = resultObject.Data.Results.First();
            Assert.Equal(record.CaseId, person.Matches.First().CaseId);
            Assert.Equal(record.ParticipantId, person.Matches.First().ParticipantId);
            Assert.Equal(state[0], person.Matches.First().State);
            Assert.Equal(record.BenefitsEndDate, person.Matches.First().BenefitsEndDate);
            Assert.Equal(record.RecentBenefitMonths, person.Matches.First().RecentBenefitMonths);
            Assert.Equal(record.ProtectLocation, person.Matches.First().ProtectLocation);
        }

        [Fact]
        public async void ApiReturnsEmptyMatchesArray()
        {
            // Arrange
            var record = FullRecord();
            var logger = Mock.Of<ILogger>();
            var body = new object[] { record };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Empty(resultObject.Data.Results[0].Matches);
        }

        [Fact]
        public async void ApiReturnsInlineErrors()
        {
            // Arrange
            var recordA = FullRecord();
            var recordB = FullRecord();
            recordB.LdsHash = "foo";
            var logger = Mock.Of<ILogger>();
            var body = new object[] { recordA, recordB };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);
            Assert.Single(resultObject.Data.Errors);
        }

        [Fact]
        public async void ApiReturnsExpectedIndices()
        {
            // Arrange
            var recordA = FullRecord();
            var recordB = FullRecord();
            // lynn,1940-08-01,000-12-3457
            recordB.LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95";
            recordB.ParticipantId = "ParticipantB";
            var logger = Mock.Of<ILogger>();
            var body = new object[] { recordA, recordB };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);
            Insert(recordB);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            var resultA = resultObject.Data.Results.Find(p => p.Index == 0);
            var resultB = resultObject.Data.Results.Find(p => p.Index == 1);

            Assert.Equal(resultA.Matches.First().ParticipantId, recordA.ParticipantId);
            Assert.Equal(resultB.Matches.First().ParticipantId, recordB.ParticipantId);
        }
    }
}
