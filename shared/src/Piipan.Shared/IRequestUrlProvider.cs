using System;
using Microsoft.AspNetCore.Http;

namespace Piipan.Shared.Http
{
    public interface IRequestUrlProvider
    {
        Uri GetBaseUrl(HttpRequest request);
    }
}