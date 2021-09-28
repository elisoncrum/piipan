using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Match.Func.Api.DataTypeHandlers;
using Piipan.Match.Func.Api.Extensions;
using Piipan.Match.Func.Api.Models;
using Piipan.Match.Shared;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Authentication;

namespace Piipan.Match.Func.Api
{
    /// <summary>
    /// Azure Function implementing orchestrator matching API.
    /// </summary>
    public class MatchApi
    {
        private readonly DbProviderFactory _dbFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly IParticipantApi _participantApi;

        public MatchApi(
            DbProviderFactory factory,
            ITokenProvider provider,
            IParticipantApi participantApi)
        {
            _dbFactory = factory;
            _tokenProvider = provider;
            _participantApi = participantApi;

            SqlMapper.AddTypeHandler(new DateTimeListHandler());
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        /// <summary>
        /// API endpoint for conducting matches across all participating states
        /// using de-identified data
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// This function is expected to be executing as a resource with read
        /// access to the per-state participant databases.
        /// </remarks>
        [FunctionName("find_matches")]
        public async Task<IActionResult> Find(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

                string subscription = req.Headers?["Ocp-Apim-Subscription-Name"];
                if (subscription != null)
                {
                    log.LogInformation("Using APIM subscription {Subscription}", subscription);
                }

                string username = req.Headers?["From"];
                if (username != null)
                {
                    log.LogInformation("on behalf of {Username}", username);
                }

                var incoming = await new StreamReader(req.Body).ReadToEndAsync();
                var request = Parse(incoming, log);

                // Top-level request validation
                (new OrchMatchRequestValidator()).ValidateAndThrow(request);

                return await FindMatches(request, log);
            }
            catch (ValidationException ex)
            {
                return ValidationErrorResponse(ex);
            }
            catch (JsonSerializationException ex)
            {
                return DeserializationErrorResponse(ex);
            }
            catch (JsonReaderException ex)
            {
                return DeserializationErrorResponse(ex);
            }
            catch (System.FormatException ex)
            {
                return DeserializationErrorResponse(ex);
            }
            catch (Exception ex)
            {
                return InternalServerErrorResponse(ex);
            }
        }

        /// <summary>
        /// API endpoint for conducting a PII match across all participating states
        /// </summary>
        /// <param name="req">incoming HTTP request</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// This function is expected to be executing as a resource with read
        /// access to the individual per-state participant databases.
        /// </remarks>
        [FunctionName("find_matches_by_pii")]
        public IActionResult FindPii(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // xxx Implement parsing, validating, and hashing of PII
            return (ActionResult)new NoContentResult();
        }

        private OrchMatchRequest Parse(string requestBody, ILogger log)
        {
            OrchMatchRequest request;

            try
            {
                request = JsonConvert.DeserializeObject<OrchMatchRequest>(requestBody);

                // An empty request body will deserialze to a null object.
                if (request is null)
                {
                    throw new JsonSerializationException("Request body must not be empty.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw ex;
            }

            return request;
        }

        private async Task<IActionResult> FindMatches(OrchMatchRequest request, ILogger log)
        {
            var response = new OrchMatchResponse();
            for (int i = 0; i < request.Data.Count; i++)
            {
                try
                {
                    var result = await PersonMatch(request.Data[i], i, log);
                    response.Data.Results.Add(result);
                }
                catch (ValidationException ex)
                {
                    // Person-level validation errors are returned in the
                    // response rather than triggering a 4xx error
                    response.Data.Errors.AddRange(HandlePersonValidationFailure(ex, i));
                }
                catch (Exception ex)
                {
                    log.LogInformation(ex.Message);
                }
            }

            return (ActionResult)new JsonResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        private async Task<OrchMatchResult> PersonMatch(RequestPerson person, int index, ILogger log)
        {
            // Person-level validation is handled here, and exception
            // is caught by app's entry point method
            var personValidator = new PersonValidator();
            personValidator.ValidateAndThrow(person);

            var states = await _participantApi.GetStates();

            var matches = await states
                .SelectManyAsync(state => _participantApi.GetParticipants(state, person.LdsHash));

            return new OrchMatchResult
            {
                Index = index,
                Matches = matches
            };
        }

        private async Task<string> ConnectionString(string database)
        {
            // Environment variables (and placeholder) established
            // during initial function app provisioning in IaC
            const string CloudName = "CloudName";
            const string DatabaseConnectionString = "DatabaseConnectionString";
            const string PasswordPlaceholder = "{password}";
            const string DatabasePlaceholder = "{database}";
            const string GovernmentCloud = "AzureUSGovernment";

            // Resource ids for open source software databases in the public and
            // US government clouds. Set the desired active cloud, then see:
            // `az cloud show --query endpoints.ossrdbmsResourceId`
            const string CommercialId = "https://ossrdbms-aad.database.windows.net";
            const string GovermentId = "https://ossrdbms-aad.database.usgovcloudapi.net";

            var resourceId = CommercialId;
            var cn = Environment.GetEnvironmentVariable(CloudName);
            if (cn == GovernmentCloud)
            {
                resourceId = GovermentId;
            }

            var builder = new NpgsqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable(DatabaseConnectionString));

            if (builder.Password == PasswordPlaceholder)
            {
                var token = await _tokenProvider.RetrieveAsync(resourceId);
                builder.Password = token.Token;
            }

            if (builder.Database == DatabasePlaceholder)
            {
                builder.Database = database;
            }

            return builder.ConnectionString;
        }

        private List<OrchMatchError> HandlePersonValidationFailure(ValidationException exception, int index)
        {
            var errors = new List<OrchMatchError>();

            foreach (var failure in exception.Errors)
            {
                // Person-level validation can result in multiple errors/failures
                errors.Add(new OrchMatchError()
                {
                    Index = index,
                    Code = failure.ErrorCode,
                    Detail = failure.ErrorMessage
                });
            }

            return errors;
        }

        private ActionResult ValidationErrorResponse(ValidationException exception)
        {
            var errResponse = new ApiErrorResponse();
            foreach (var failure in exception.Errors)
            {
                errResponse.Errors.Add(new ApiHttpError()
                {
                    Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                    Title = failure.ErrorCode,
                    Detail = failure.ErrorMessage
                });
            }
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }

        private ActionResult DeserializationErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                Title = Convert.ToString(ex.GetType()),
                Detail = ex.Message
            });
            return (ActionResult)new BadRequestObjectResult(errResponse);
        }

        private ActionResult InternalServerErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.InternalServerError),
                Title = ex.GetType().Name,
                Detail = ex.Message
            });
            return (ActionResult)new JsonResult(errResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
