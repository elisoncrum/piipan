using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Piipan.Shared.Logging
{
    public class AuthenticationLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public const string CLAIMS_LOGGED_KEY = "CLAIMS_LOGGED";

        public AuthenticationLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<AuthenticationLoggingMiddleware> logger)
        {
            if (!context.Session.GetInt32(CLAIMS_LOGGED_KEY).HasValue)
            {
                foreach (var claim in context.User.Claims)
                {
                    logger.LogInformation($"[CLAIM] {claim.Type}: {claim.Value}");
                }
            }

            context.Session.SetInt32(CLAIMS_LOGGED_KEY, 1);

            await _next(context);
        }
    }
}