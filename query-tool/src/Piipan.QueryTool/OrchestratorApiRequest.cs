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

        public async Task<MatchResponse> Match(PiiRecord query)
        {
            const string Endpoint = "query";

            var request = new MatchRequest { Query = query };
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

        public async Task<LookupResponse> Lookup(string lookupId)
        {
            const string Endpoint = "lookup_ids";

            var uri = new Uri(_baseUri, $"{Endpoint}/{lookupId}");
            var response = await _apiClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var lookupJson = await response.Content.ReadAsStringAsync();
            var lookupResponse = JsonSerializer.Deserialize<LookupResponse>(lookupJson);

            return lookupResponse;
        }
    }

    public class MatchResponse
    {
        [JsonPropertyName("lookup_id")]
        public virtual string lookupId { get; set; }
        public virtual List<PiiRecord> matches { get; set; }
    }

    public class LookupResponse
    {
        [JsonPropertyName("data")]
        public virtual PiiRecord data { get; set; }
    }
}
