using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    public class OrchestratorApiRequest
    {
        public string RequestUrl;
        public PiiRecord Query;

        public async Task<string> SendQuery(string url, PiiRecord query)
        {
            RequestUrl = url;
            Query = query;
            Console.WriteLine(query.LastName);
            return await QueryOrchestrator();
        }

        private static readonly HttpClient client = new HttpClient();
        public string ResponseText { get; private set; }

        private async Task<string> QueryOrchestrator()
        {
            try
            {
                var message = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
                var resp = await client.SendAsync(message);
                var streamTask = await resp.Content.ReadAsStreamAsync();
                var json = await JsonSerializer.DeserializeAsync<OrchestratorApiResponse>(streamTask);
                ResponseText = json.text;
                return ResponseText;
            }
            catch (Exception e)
            {
                ResponseText = e.Message;
                return ResponseText;
            }
        }
    }

    public class OrchestratorApiResponse
    {
        public string text { get; set; }
    }
}
