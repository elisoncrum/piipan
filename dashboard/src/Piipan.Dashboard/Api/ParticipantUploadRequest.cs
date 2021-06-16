using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Piipan.Shared.Authentication;

namespace Piipan.Dashboard
{
    namespace Api
    {
        public interface IParticipantUploadRequest
        {
            public Task<ParticipantUploadResponse> Get(string url);
        }
        public class ParticipantUploadRequest : IParticipantUploadRequest
        {
            private readonly IAuthorizedApiClient _apiClient;

            public ParticipantUploadRequest(IAuthorizedApiClient apiClient)
            {
                _apiClient = apiClient;
            }
            public async Task<ParticipantUploadResponse> Get(string url)
            {
                try
                {
                    var response = await _apiClient.GetAsync(new Uri(url));
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
