using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.QueryTool
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, 
            ILogger<RequestLoggingMiddleware> logger,
            IClaimsProvider claimsProvider)
        {
            logger.LogInformation($"{claimsProvider.GetEmail(context.User)} {context.Request.Method} {context.Request.Path}");
            await _next(context);
        }
    }
}