using System;
using Microsoft.AspNetCore.Http;

namespace Piipan.Shared.Http
{
    public class RequestUrlProvider : IRequestUrlProvider
    {
        public Uri GetBaseUrl(HttpRequest request)
        {
            return new Uri( $"{request.Scheme}://{request.Host.ToString()}");
        }
    }
}