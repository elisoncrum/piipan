using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Match.Shared;
using Piipan.Shared.Authentication;

namespace Piipan.Match.Orchestrator
{
    /// <summary>
    /// Azure Function implementing orchestrator matching API.
    /// </summary>
    public class Api
    {
        private readonly IAuthorizedApiClient _apiClient;
        private readonly ITableStorage<QueryEntity> _lookupStorage;

        private readonly int MaxPersonsLimit = 50;

        public Api(IAuthorizedApiClient apiClient, ITableStorage<QueryEntity> lookupStorage)
        {
            _apiClient = apiClient;
            _lookupStorage = lookupStorage;
        }

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
        public async Task<IActionResult> Query(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

                var incoming = await new StreamReader(req.Body).ReadToEndAsync();
                var request = Parse(incoming, log);
                if (!request.Query.Any())
                {
                    // Incoming request could not be deserialized into MatchQueryResponse
                    // XXX return validation messages
                    var errResponse = new ApiErrorResponse();
                    errResponse.Errors.Add(new ApiHttpError()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Title = "Invalid Request",
                        Detail = "Request body contains no query property"
                    });
                    return (ActionResult)new BadRequestObjectResult(errResponse);
                }

                if (request.Query.Count > MaxPersonsLimit)
                {
                    // Incoming request list is longer than the max allowed
                    var errResponse = new ApiErrorResponse();
                    errResponse.Errors.Add(new ApiHttpError() {
                        StatusCode = HttpStatusCode.BadRequest,
                        Title = "Persons Limit Exceeded",
                        Detail = $"Persons in request cannot exceed {MaxPersonsLimit}"
                    });
                    return (ActionResult)new BadRequestObjectResult(errResponse);
                }

                if (!Validate(request, log))
                {
                    // Request successfully deserialized but contains invalid properties
                    // XXX return validation messages
                    var errResponse = new ApiErrorResponse();
                    errResponse.Errors.Add(new ApiHttpError()
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Title = "Invalid Request",
                        Detail = "Request data contains invalid properties"
                    });
                    return (ActionResult)new BadRequestObjectResult(errResponse);
                }

                var orchResponse = new MatchResponse();
                for (int i = 0; i < request.Query.Count; i++)
                {
                    var stateResponse = new StateMatchQueryResponse();
                    try
                    {
                        var query = request.Query[i];
                        StateMatchQueryRequest stateRequest = new StateMatchQueryRequest();
                        stateRequest.Query = new StateMatchQuery {
                            Last = query.Last,
                            First = query.First,
                            Middle = query.Middle,
                            Dob = query.Dob,
                            Ssn = query.Ssn
                        };
                        stateResponse.Index = i;
                        stateResponse.Matches = await Match(stateRequest, log);

                        if (stateResponse.Matches.Count > 0)
                        {
                            stateResponse.LookupId = await Lookup.Save(query, _lookupStorage, log);
                        }
                        orchResponse.Data.Results.Add(stateResponse);
                    }
                    catch (Exception ex)
                    {
                        // Exception when attempting state-level matches
                        log.LogError(ex.Message);

                        orchResponse.Data.Errors.Add(new MatchDataError() {
                            Index = i,
                            Code = ex.GetType().Name,
                            Detail = ex.Message
                        });
                    }

                }
                return (ActionResult)new JsonResult(orchResponse);
            }
            catch (Exception topLevelEx)
            {
                log.LogError(topLevelEx.Message);
                var errResponse = new ApiErrorResponse();
                errResponse.Errors.Add(new ApiHttpError()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Title = topLevelEx.GetType().Name,
                    Detail = topLevelEx.Message
                });
                return (ActionResult)new JsonResult(errResponse)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// API endpoint for retrieving a MatchQuery using a lookup ID
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="lookupId">lookup ID string (pulled from route)</param>
        /// <param name="log">handle to the function log</param>
        [FunctionName("lookup_ids")]
        public async Task<IActionResult> LookupIds(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "lookup_ids/{lookupId}")] HttpRequest req,
            string lookupId,
            ILogger log)
        {
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            LookupResponse response = new LookupResponse { Data = null };
            response.Data = await Lookup.Retrieve(lookupId, _lookupStorage, log);

            return (ActionResult)new JsonResult(response);
        }

        private MatchQueryRequest Parse(string requestBody, ILogger log)
        {
            // Assume failure
            MatchQueryRequest request = new MatchQueryRequest();

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

        private bool Validate(MatchQueryRequest request, ILogger log)
        {
            MatchQueryRequestValidator validator = new MatchQueryRequestValidator();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                log.LogError(result.ToString());
            }

            return result.IsValid;
        }

        private IEnumerable<Uri> StateApiUris()
        {
            const string StateApiUriStrings = "StateApiUriStrings";

            // XXX Validate input
            IEnumerable<Uri> uris = JsonConvert.DeserializeObject<IEnumerable<Uri>>(
                Environment.GetEnvironmentVariable(StateApiUriStrings));

            return uris;
        }

        private async Task<StateMatchQueryResponse> MatchState(Uri uri, StateMatchQueryRequest request, ILogger log)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request));
            var response = await _apiClient.PostAsync(uri, content);

            response.EnsureSuccessStatusCode();

            var matchResponse = await response.Content.ReadAsAsync<StateMatchQueryResponse>();

            return matchResponse;
        }

        private async Task<List<PiiRecord>> Match(StateMatchQueryRequest request, ILogger log)
        {
            var matches = new List<PiiRecord>();
            var stateRequests = new List<Task<StateMatchQueryResponse>>();
            var stateApiUris = StateApiUris();

            foreach (var uri in stateApiUris)
            {
                stateRequests.Add(MatchState(uri, request, log));
            }

            await Task.WhenAll(stateRequests.ToArray());

            foreach (var stateRequest in stateRequests)
            {
                matches.AddRange(stateRequest.Result.Matches);
            }

            return matches;
        }
    }
}
