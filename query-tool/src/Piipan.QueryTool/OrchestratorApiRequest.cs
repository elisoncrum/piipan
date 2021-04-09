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
        public Dictionary<string, IQueryable> Query = new Dictionary<string, IQueryable>();
        private readonly IAuthorizedApiClient _apiClient;

        public async Task<Dictionary<string, object>> SendQuery(string url, Lookup query)
        {
            RequestUrl = url;
            Query.Add("query", query);
            return await QueryOrchestrator();
        }

        public async Task<Dictionary<string, object>> SendQuery(string url, PiiRecord query)
        {
            RequestUrl = url;
            Query.Add("query", query);
            return await QueryOrchestrator();
        }

        public Dictionary<string, object> ResponseDict { get; private set; }

        private async Task<Dictionary<string, object>> QueryOrchestrator()
        {
            try
            {
                _logger.LogInformation("Querying Orchestrator API");

                if (Query["query"] is Lookup)
                {
                    List<PiiRecord> matches;
                    matches = await SendLookupIdQuery();
                    ResponseDict.Add("matches", matches);
                }
                else if (Query["query"] is PiiRecord)
                {
                    ResponseDict = await SendPiiQuery();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
            }
            return ResponseDict;
        }

        private async Task<List<PiiRecord>> SendLookupIdQuery()
        {
            List<PiiRecord> Matches = new List<PiiRecord>();

            try
            {
                _logger.LogInformation("Sending LookupID query");
                var requestUri = new Uri(RequestUrl);
                var jsonString = JsonSerializer.Serialize(Query);
                Console.WriteLine(jsonString);
                var lookup = Query["query"] as Lookup;
                requestUri = new Uri(requestUri, $"lookup_ids/{lookup.LookupId}");
                var resp = await _apiClient.GetAsync(requestUri);
                Console.WriteLine(resp.ToString());
                Console.WriteLine(resp.Content.ReadAsStringAsync().Result);
                var content = await resp.Content.ReadAsStringAsync();
                var streamTask = await resp.Content.ReadAsStreamAsync();
                var json = await JsonSerializer.DeserializeAsync<LookupApiResponse>(streamTask);
                Console.WriteLine(json);
                if (json.data != null)
                {
                    Matches.Add(json.data);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
            }

            return Matches;
        }

        private async Task<Dictionary<string, object>> SendPiiQuery()
        {
            Dictionary<string, object> Response = new Dictionary<string, object>();

            try
            {
                _logger.LogInformation("Sending PII-based query");
                var requestUri = new Uri(RequestUrl);
                var jsonString = JsonSerializer.Serialize(Query);
                var requestBody = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var resp = await _apiClient.PostAsync(requestUri, requestBody);
                var streamTask = await resp.Content.ReadAsStreamAsync();
                var json = await JsonSerializer.DeserializeAsync<PiiApiResponse>(streamTask);
                Response.Add("matches", json.matches);
                Response.Add("lookupId", json.lookupId);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
            }

            return Response;
        }
    }

    public class PiiApiResponse
    {
        public virtual string lookupId { get; set; }
        public virtual List<PiiRecord> matches { get; set; }
    }

    public class LookupApiResponse
    {
        public virtual PiiRecord data { get; set; }
    }
}
