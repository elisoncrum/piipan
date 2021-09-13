// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Metrics.Api;
using Piipan.Shared.Authentication;

namespace Piipan.Metrics.Func.Collect
{
    public class BulkUploadMetrics
    {
        private readonly IParticipantUploadApi _participantUploadApi;

        public BulkUploadMetrics(IParticipantUploadApi participantUploadApi)
        {
            _participantUploadApi = participantUploadApi;
        }

        /// <summary>
        /// Listens for BulkUpload events when users upload participants;
        /// write meta info to Metrics database
        /// </summary>
        /// <param name="eventGridEvent">storage container blob creation event</param>
        /// <param name="log">handle to the function log</param>

        [FunctionName("BulkUploadMetrics")]
        public void Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            try
            {
                string state = ParseState(eventGridEvent);
                DateTime uploadedAt = eventGridEvent.EventTime;

                int nRows = _participantUploadApi.AddUpload(state, uploadedAt);

                log.LogInformation(String.Format("Number of rows inserted={0}", nRows));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        private string ParseState(EventGridEvent eventGridEvent)
        {
            var jsondata = JsonConvert.SerializeObject(eventGridEvent.Data);
            var tmp = new { url = "" };
            var data = JsonConvert.DeserializeAnonymousType(jsondata, tmp);

            Regex regex = new Regex("^https://([a-z]+)upload");
            Match match = regex.Match(data.url);

            if (match.Success)
            {
                var val = match.Groups[1].Value;
                return val.Substring(val.Length - 2); // parses abbreviation from match value
            }
            else
            {
                throw new FormatException("State not found");
            }
        }
    }
}
