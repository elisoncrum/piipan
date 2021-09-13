using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Metrics.Api;
using Piipan.Metrics.Func.Api.Builders;

#nullable enable

namespace Piipan.Metrics.Func.Api
{
    /// <summary>
    /// implements getting latest upload from each state.
    /// </summary>
    public class GetLastUpload
    {
        private readonly IParticipantUploadApi _participantUploadApi;
        private readonly IMetaBuilder _metaBuilder;

        public GetLastUpload(
            IParticipantUploadApi participantUploadApi,
            IMetaBuilder metaBuilder)
        {
            _participantUploadApi = participantUploadApi;
            _metaBuilder = metaBuilder;
        }

        /// <summary>
        /// Azure Function implementing getting latest upload from each state.
        /// </summary>
        [FunctionName("GetLastUpload")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            try
            {
                var data = _participantUploadApi.GetLatestUploadsByState();
                var meta = _metaBuilder.Build();

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
