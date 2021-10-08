using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Piipan.Shared.Authentication;

namespace Piipan.Shared.Http
{
    /// <summary>
    /// Client for making authorized API calls within the Piipan system
    /// </summary>
    public class AuthorizedJsonApiClient<T> : IAuthorizedApiClient<T>
    {
        private IHttpClientFactory _clientFactory;
        private ITokenProvider<T> _tokenProvider;
        private string _accept = "application/json";

        /// <summary>
        /// Creates a new instance of AuthorizedJsonApiClient
        /// </summary>
        /// <param name="clientFactory">an instance of IHttpClientFactory</param>
        /// <param name="tokenProvider">an instance of ITokenProvider</param>
        public AuthorizedJsonApiClient(IHttpClientFactory clientFactory,
            ITokenProvider<T> tokenProvider)
        {
            _clientFactory = clientFactory;
            _tokenProvider = tokenProvider;
        }

        private async Task<HttpRequestMessage> PrepareRequest(string path, HttpMethod method)
        {
            var token = await _tokenProvider.RetrieveAsync();
            var httpRequestMessage = new HttpRequestMessage(method, path)
            {
                Headers = 
                {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {token}" },
                    { HttpRequestHeader.Accept.ToString(), _accept }
                }
            };

            return httpRequestMessage;
        }

        public async Task<HttpResponseMessage> PostAsync(string path, StringContent body)
        {
            var requestMessage = await PrepareRequest(path, HttpMethod.Post);
            requestMessage.Content = body;

            var response = await Client().SendAsync(requestMessage);

            return response;
        }

        public async Task<HttpResponseMessage> GetAsync(string path)
        {
            var requestMessage = await PrepareRequest(path, HttpMethod.Get);
            var response = await Client().SendAsync(requestMessage);

            return response;
        }

        private HttpClient Client()
        {
            var clientName = typeof(T).Name;
            return _clientFactory.CreateClient(clientName);
        }
    }
}
