using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Piipan.Shared.Http
{
    public class ProxiedRequestUrlProvider : IRequestUrlProvider
    {
        public Uri GetBaseUrl(HttpRequest request)
        {
            var proto = request.Headers.Single(h => h.Key == "X-Forwarded-Proto").Value;
            var host = request.Headers.Single(h => h.Key == "X-Forwarded-Host").Value;
            return new Uri($"{proto}://{host}");
        }
    }
}