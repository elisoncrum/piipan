using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Metrics.Func.Api.Builders;
using Piipan.Metrics.Func.Api.Extensions;
using Piipan.Metrics.Core.Services;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Metrics.Func.Api
{
    public class GetParticipantUploads
    {
        private readonly IParticipantUploadApi _participantUploadApi;
        private readonly IMetaBuilder _metaBuilder;

        public GetParticipantUploads(
            IParticipantUploadApi participantUploadApi,
            IMetaBuilder metaBuilder)
        {
            _participantUploadApi = participantUploadApi;
            _metaBuilder = metaBuilder;
        }

        [FunctionName("GetParticipantUploads")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            try
            {
                var perPage = req.Query.ParseInt("perPage", 50);
                var page = req.Query.ParseInt("page", 1);
                var state = req.Query.ParseString("state");
                var offset = perPage * (page - 1);

                var data = _participantUploadApi.GetUploads(state, perPage, offset);

                var meta = _metaBuilder
                    .SetPage(page)
                    .SetPerPage(perPage)
                    .SetState(state)
                    .Build();

                var response = new GetParticipantUploadsResponse
                {
                    Data = data,
                    Meta = meta
                };

                return (ActionResult)new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
