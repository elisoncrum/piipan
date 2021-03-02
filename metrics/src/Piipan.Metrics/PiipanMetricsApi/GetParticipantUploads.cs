using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Models;
using Piipan.Metrics.Api.Serializers;
using Piipan.Metrics.Api.Builders;
using Piipan.Metrics.Api.DataAccessObjects;
using Piipan.Metrics.Api.Extensions;

#nullable enable

namespace Piipan.Metrics.Api
{
    public class GetParticipantUploads
    {
        private readonly IParticipantUploadDao _participantUploadDao;
        private readonly IMetaBuilder _metaBuilder;

        public GetParticipantUploads(IParticipantUploadDao participantUploadDao,
            IMetaBuilder metaBuilder)
        {
            _participantUploadDao = participantUploadDao;
            _metaBuilder = metaBuilder;
        }

        [FunctionName("GetParticipantUploads")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var perPage = req.Query.ParseInt("perPage", 50);
                var page = req.Query.ParseInt("page", 1);
                var state = req.Query.ParseString("state");
                var offset = perPage * (page - 1);
             
                var uploads = _participantUploadDao.GetParticipantUploadsForState(state, perPage, offset);
                var meta = _metaBuilder
                    .SetPage(page)
                    .SetPerPage(perPage)
                    .SetState(state)
                    .Build();

                var response = new ParticipantUploadsResponse(
                    uploads,
                    meta
                );

                return new OkObjectResult(
                    JsonConvert.SerializeObject(response, Formatting.Indented)
                );
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
