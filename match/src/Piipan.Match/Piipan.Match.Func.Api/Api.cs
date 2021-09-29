using System;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Match.Func.Api.DataTypeHandlers;
using Piipan.Match.Func.Api.Models;
using Piipan.Match.Func.Api.Parsers;
using Piipan.Match.Func.Api.Resolvers;
using Piipan.Match.Shared;

namespace Piipan.Match.Func.Api
{
    /// <summary>
    /// Azure Function implementing orchestrator matching API.
    /// </summary>
    public class MatchApi
    {
        private readonly IMatchResolver _matchResolver;
        private readonly IStreamParser<OrchMatchRequest> _requestParser;

        public MatchApi(
            IMatchResolver matchResolver,
            IStreamParser<OrchMatchRequest> requestParser)
        {
            _matchResolver = matchResolver;
            _requestParser = requestParser;

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

                string subscription = req.Headers["Ocp-Apim-Subscription-Name"];
                if (subscription != null)
                {
                    log.LogInformation("Using APIM subscription {Subscription}", subscription);
                }

                string username = req.Headers["From"];
                if (username != null)
                {
                    log.LogInformation("on behalf of {Username}", username);
                }
                
                var request = await _requestParser.Parse(req.Body);
                var response = await _matchResolver.ResolveMatches(request);

                return (ActionResult)new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            catch (StreamParserException ex)
            {
                return DeserializationErrorResponse(ex);
            }
            catch (ValidationException ex)
            {
                return ValidationErrorResponse(ex);
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
