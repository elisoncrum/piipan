using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Piipan.Dashboard
{
    namespace Api
    {
        public class ParticipantUploadRequest
        {
            private readonly HttpClient Client;

            public ParticipantUploadRequest(HttpClient _client)
            {
                Client = _client;
            }
            public async Task<ParticipantUploadResponse> Get(string url)
            {
                try
                {
                    var response = await Client.GetAsync(url);
                    var body = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ParticipantUploadResponse>(body);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught: {0}", e.Message);
                    throw e;
                }
            }
        }

        public class ParticipantUploadResponse
        {
            public ParticipantUploadResponseMeta meta { get; set; }
            public List<ParticipantUpload> data { get; set; }
        }

        public class ParticipantUploadResponseMeta
        {
            public int page { get; set; }
            public int perPage { get; set; }
            public Int64 total { get; set; }
            public string nextPage { get; set; }
            public string prevPage { get; set; }
        }
    }
}
