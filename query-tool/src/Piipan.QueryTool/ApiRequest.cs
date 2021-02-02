using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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
            return await QueryOrchestrator();
        }

        private static readonly HttpClient client = new HttpClient();
        public string ResponseText { get; private set; }

        private async Task<string> QueryOrchestrator()
        {
            try
            {
                var message = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
                var jsonString = JsonSerializer.Serialize(Query);
                message.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
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
