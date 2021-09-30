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
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Resolvers;
using Piipan.Match.Func.Api.DataTypeHandlers;
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
        /// <param name="logger">handle to the function log</param>
        /// <remarks>
        /// This function is expected to be executing as a resource with read
        /// access to the per-state participant databases.
        /// </remarks>
        [FunctionName("find_matches")]
        public async Task<IActionResult> Find(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            try
            {
                LogRequest(logger, req);
                
                var request = await _requestParser.Parse(req.Body);
                var response = await _matchResolver.ResolveMatches(request);

                return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
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

        private void LogRequest(ILogger logger, HttpRequest request)
        {
            logger.LogInformation("Executing request from user {User}", request.HttpContext?.User.Identity.Name);

            string subscription = request.Headers["Ocp-Apim-Subscription-Name"];
            if (subscription != null)
            {
                logger.LogInformation("Using APIM subscription {Subscription}", subscription);
            }

            string username = request.Headers["From"];
            if (username != null)
            {
                logger.LogInformation("on behalf of {Username}", username);
            }
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
