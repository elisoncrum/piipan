using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Authentication;

namespace Piipan.QueryTool
{
    public class OrchestratorApiRequest
    {
        private readonly ILogger _logger;
        private readonly IAuthorizedApiClient _apiClient;
        private readonly Uri _baseUri;

        public OrchestratorApiRequest(IAuthorizedApiClient apiClient, Uri baseUri, ILogger logger)
        {
            _apiClient = apiClient;
            _baseUri = baseUri;
            _logger = logger;
        }

        public async Task<MatchResponse> Match(MatchRequestRecord record)
        {
            const string Endpoint = "find_matches";

            var request = new MatchRequest { Data = new List<MatchRequestRecord> { record } };
            var json = JsonSerializer.Serialize(request);
            var response = await _apiClient.PostAsync(
                new Uri(_baseUri, Endpoint),
                new StringContent(json)
            );

            response.EnsureSuccessStatusCode();

            var matchJson = await response.Content.ReadAsStringAsync();
            var matchResponse = JsonSerializer.Deserialize<MatchResponse>(matchJson);

            return matchResponse;
        }
    }

    public class MatchError
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }
    }
}
