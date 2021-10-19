using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Piipan.Shared.Http
{
    public interface IAuthorizedApiClient<T>
    {
        /// <summary>
        /// Send an asynchronous POST request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        /// <param name="body">object to be sent as request body</param>
        Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body);

        /// <summary>
        /// Send an asynchronous POST request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        /// <param name="body">object to be sent as request body</param>
        /// <param name="headerFactory">callback which supplies additional headers to be included in the outbound request</param>
        Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, Func<IEnumerable<(string, string)>> headerFactory);

        /// <summary>
        /// Send an asynchronous GET request to an API endpoint
        /// </summary>
        /// <param name="path">path portion of the API endpoint</param>
        Task<TResponse> GetAsync<TResponse>(string path);
    }
}