using System.Net;
using System.Net.Http;
using System.Text.Json;
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

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body)
        {
            var requestMessage = await PrepareRequest(path, HttpMethod.Post);
            
            var json = JsonSerializer.Serialize(body);
            requestMessage.Content = new StringContent(json);

            var response = await Client().SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            var responseContentJson = await response.Content.ReadAsStringAsync();
            
            return JsonSerializer.Deserialize<TResponse>(responseContentJson);
        }

        public async Task<TResponse> GetAsync<TResponse>(string path)
        {
            var requestMessage = await PrepareRequest(path, HttpMethod.Get);
            var response = await Client().SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            var responseContentJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TResponse>(responseContentJson);
        }

        private HttpClient Client()
        {
            var clientName = typeof(T).Name;
            return _clientFactory.CreateClient(clientName);
        }
    }
}
