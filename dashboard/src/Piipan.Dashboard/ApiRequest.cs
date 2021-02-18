
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Piipan.Dashboard
{
    public class ApiRequest
    {
        private static HttpClient _client;
        public static async Task<ApiResponse> Get(string url)
        {
            try
            {
                _client = new HttpClient();
                string response = await _client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<ApiResponse>(response);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught: {0}", e.Message);
                throw e;
            }
        }
    }
    public class Meta
    {
        public virtual int page { get; set; }
        public virtual int perPage { get; set; }
        public virtual Int64 total { get; set; }
        public virtual string nextPage { get; set; }
        public virtual string prevPage { get; set; }
    }
    public class ApiResponse
    {
        public virtual Meta meta { get; set; }
        public virtual List<ParticipantUpload> data { get; set; }
    }
}
