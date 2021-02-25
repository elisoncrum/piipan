using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Piipan.Shared.Authentication;

namespace Piipan.QueryTool
{
    public class OrchestratorApiRequest
    {
        public string RequestUrl;
        public Dictionary<string, PiiRecord> Query = new Dictionary<string, PiiRecord>();
        private static HttpClient _client;

        public async Task<List<PiiRecord>> SendQuery(string url, PiiRecord query)
        {
            RequestUrl = url;
            Query.Add("query", query);
            _client = new HttpClient();
            return await QueryOrchestrator();
        }

        public async Task<List<PiiRecord>> SendQuery(string url, PiiRecord query, HttpClient client)
        {
            RequestUrl = url;
            Query.Add("query", query);
            _client = client;
            return await QueryOrchestrator();
        }

        public List<PiiRecord> Matches { get; private set; }

        private async Task<List<PiiRecord>> QueryOrchestrator()
        {
            {
                var requestUri = new Uri(RequestUrl);
                var tokenProvider = new EasyAuthTokenProvider();
                var authorizedClient = new AuthorizedJsonApiClient(_client, tokenProvider);
                var jsonString = JsonSerializer.Serialize(Query);
                var requestBody = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var resp = await authorizedClient.PostAsync(requestUri, requestBody);
                var streamTask = await resp.Content.ReadAsStreamAsync();
                var json = await JsonSerializer.DeserializeAsync<OrchestratorApiResponse>(streamTask);
                Matches = json.matches;
                return Matches;
            }
        }
    }

    public class OrchestratorApiResponse
    {
        public virtual List<PiiRecord> matches { get; set; }
    }
}
