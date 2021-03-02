using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace Piipan.Shared.Authentication
{
    public interface ITokenProvider
    {
        Task<AccessToken> RetrieveAsync(string resourceUri);
    }

    public class CliTokenProvider : ITokenProvider
    {
        public async Task<AccessToken> RetrieveAsync(string resourceUri)
        {
            var tokenCredential = new AzureCliCredential();
            var context = new TokenRequestContext(new[] { resourceUri });
            var tokenTask = await tokenCredential.GetTokenAsync(context);

            return tokenTask;
        }
    }

    public class EasyAuthTokenProvider : ITokenProvider
    {
        public async Task<AccessToken> RetrieveAsync(string appUri)
        {
            var tokenCredential = new ManagedIdentityCredential();
            var context = new TokenRequestContext(new[] { appUri });
            var tokenTask = await tokenCredential.GetTokenAsync(context);

            return tokenTask;
        }
    }

    public interface IAuthorizedApiClient
    {
        /// <summary>
        /// Send an asynchronous POST request to an API endpoint
        /// </summary>
        /// <param name="uri">URI of the API endpoint</param>
        /// <param name="body">Request body</param>
        Task<HttpResponseMessage> PostAsync(Uri uri, StringContent body);

        /// <summary>
        /// Send an asynchronous GET request to an API endpoint
        /// </summary>
        /// <param name="uri">URI of the API endpoint</param>
        /// <remarks>Not yet implemented</remarks>
        Task<HttpResponseMessage> GetAsync(Uri uri);
    }

    public class AuthorizedJsonApiClient : IAuthorizedApiClient
    {
        private HttpClient _client;
        private ITokenProvider _tokenProvider;
        private string _accept = "application/json";

        /// <summary>
        /// Client for making authorized API calls within the Piipan system
        /// </summary>
        /// <param name="client">HttpClient instance</param>
        /// <param name="tokenProvider">An implmentation of ITokenProvider used to retrive access token</param>
        /// <remarks>
        /// -- Remarks --
        /// </remarks>
        public AuthorizedJsonApiClient(HttpClient client, ITokenProvider tokenProvider)
        {
            _client = client;
            _tokenProvider = tokenProvider;
        }

        private async Task<HttpRequestMessage> PrepareRequest(Uri uri, HttpMethod method)
        {
            var token = await _tokenProvider.RetrieveAsync($"https://{uri.Host}");
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = uri,
                Headers = {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {token.Token}" },
                    { HttpRequestHeader.Accept.ToString(), _accept }
                }
            };

            return httpRequestMessage;
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri, StringContent body)
        {
            var requestMessage = await PrepareRequest(uri, HttpMethod.Post);
            requestMessage.Content = body;

            var response = await _client.SendAsync(requestMessage);

            return response;
        }

        public Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            throw new NotImplementedException("GetAsync method not yet implemented.");
        }
    }
}
