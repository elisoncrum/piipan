using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Piipan.Shared.Http
{
    public class ProxiedRequestUrlProvider : IRequestUrlProvider
    {
        private readonly ILogger<ProxiedRequestUrlProvider> _logger;

        public ProxiedRequestUrlProvider(ILogger<ProxiedRequestUrlProvider> logger)
        {
            _logger = logger;
        }

        public Uri GetBaseUrl(HttpRequest request)
        {
            try
            {
                var proto = request.Headers.Single(h => h.Key == "X-Forwarded-Proto").Value;
                var host = request.Headers.Single(h => h.Key == "X-Forwarded-Host").Value;
                return new Uri($"{proto}://{host}");
            }
            catch (InvalidOperationException)
            {
                _logger.LogError($"Unable to extract X-Forwarded-Proto and/or X-Forwarded-Host from request headers!");
                throw;
            }
        }
    }
}