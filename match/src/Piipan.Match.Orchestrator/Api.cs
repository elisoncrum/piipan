using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Piipan.Match.Orchestrator
{
    /// <summary>
    /// Azure Function implementing orchestrator matching API.
    /// </summary>
    public static class Api
    {
        static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// API endpoint for conducting a PII match across all participating states
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// This function is expected to be executing as a resource with query
        /// access to the individual per-state API resources.
        /// </remarks>
        [FunctionName("query")]
        public static async Task<IActionResult> Query(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var incoming = await new StreamReader(req.Body).ReadToEndAsync();
            var request = Parse(incoming, log);
            if (request.Query == null)
            {
                // Incoming request could not be deserialized into MatchQueryResponse
                // XXX return validation messages
                return (ActionResult)new BadRequestResult();
            }

            if (!Validate(request, log))
            {
                // Request successfully deserialized but contains invalid properties
                // XXX return validation messages
                return (ActionResult)new BadRequestResult();
            }

            var response = new MatchQueryResponse();
            try
            {
                response.Matches = await Match(request, client, log);
            }
            catch (Exception ex)
            {
                // Exception when attempting state-level matches, fail with 500
                // XXX fine-grained, per-state handling
                log.LogError(ex.Message);
                return (ActionResult)new InternalServerErrorResult();
            }

            return (ActionResult)new JsonResult(response);
        }

        internal static MatchQueryRequest Parse(string requestBody, ILogger log)
        {
            // Assume failure
            MatchQueryRequest request = new MatchQueryRequest { Query = null };

            try
            {
                request = JsonConvert.DeserializeObject<MatchQueryRequest>(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return request;
        }

        internal static bool Validate(MatchQueryRequest request, ILogger log)
        {
            MatchQueryRequestValidator validator = new MatchQueryRequestValidator();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                log.LogError(result.ToString());
            }

            return result.IsValid;
        }

        internal static IEnumerable<Uri> StateApiBaseUris()
        {
            const string StateApiHostStrings = "StateApiHostStrings";

            // XXX Validate input
            IEnumerable<Uri> uris = JsonConvert.DeserializeObject<IEnumerable<Uri>>(
                Environment.GetEnvironmentVariable(StateApiHostStrings));

            return uris;
        }

        internal async static Task<HttpRequestMessage> RequestMessage(Uri baseUri, Uri apiUri, MatchQueryRequest request, ILogger log)
        {
            // Retrieve authentication token via Azure AD Easy Auth. Passing an empty
            // string to `AzureServiceTokenProvider` uses system-assigned identity.
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            // Passed URI must match AAD app URI exactly; AAD app URI does not contain
            // a trailing slash, but `Uri.AbsoluteUri` will add one. 
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(baseUri.AbsoluteUri.TrimEnd('/'));

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = apiUri,
                Headers = {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {accessToken}" },
                    { HttpRequestHeader.Accept.ToString(), "application/json" }
                },
                Content = new StringContent(JsonConvert.SerializeObject(request))
            };

            return httpRequestMessage;
        }

        internal async static Task<MatchQueryResponse> MatchState(Uri baseUri, MatchQueryRequest request, HttpClient client, ILogger log)
        {
            const string StateApiEndpointPath = "StateApiEndpointPath";
            Uri apiUri = new Uri(baseUri, Environment.GetEnvironmentVariable(StateApiEndpointPath));
            HttpRequestMessage requestMessage = await RequestMessage(baseUri, apiUri, request, log);

            var response = client.SendAsync(requestMessage).Result;

            // Exception caught by `Query`
            response.EnsureSuccessStatusCode();

            var matchResponse = await response.Content.ReadAsAsync<MatchQueryResponse>();

            return matchResponse;
        }

        internal async static Task<List<PiiRecord>> Match(MatchQueryRequest request, HttpClient client, ILogger log)
        {
            List<PiiRecord> matches = new List<PiiRecord>();
            var stateApiBaseUris = StateApiBaseUris();

            // Loop through each state, compile results
            foreach (var host in stateApiBaseUris)
            {
                var stateMatches = await MatchState(host, request, client, log);
                matches.AddRange(stateMatches.Matches);
            }

            return matches;
        }
    }
}
