using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Authentication;

namespace Piipan.QueryTool
{
    public class OrchestratorApiRequest
    {
        private readonly ILogger _logger;

        public OrchestratorApiRequest(IAuthorizedApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public string RequestUrl;
        public Dictionary<string, object> Query = new Dictionary<string, object>();
        private readonly IAuthorizedApiClient _apiClient;

        public async Task<List<PiiRecord>> SendQuery(string url, IQueryable query)
        {
            RequestUrl = url;

            if (query is PiiRecord) {
                Query.Add("query", query as PiiRecord);
            } else if (query is Lookup) {
                Query.Add("query", query as Lookup);
            }
            return await QueryOrchestrator();
        }

        public List<PiiRecord> Matches { get; private set; }

        private async Task<List<PiiRecord>> QueryOrchestrator()
        {
            try
            {
                _logger.LogInformation("Querying Orchestrator API");
                var requestUri = new Uri(RequestUrl);
                var jsonString = JsonSerializer.Serialize(Query);
                var requestBody = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var resp = await _apiClient.PostAsync(requestUri, requestBody);
                var streamTask = await resp.Content.ReadAsStreamAsync();
                var json = await JsonSerializer.DeserializeAsync<OrchestratorApiResponse>(streamTask);
                Matches = json.matches;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
            }
            return Matches;
        }
    }

    public class OrchestratorApiResponse
    {
        public virtual List<PiiRecord> matches { get; set; }
    }
}
