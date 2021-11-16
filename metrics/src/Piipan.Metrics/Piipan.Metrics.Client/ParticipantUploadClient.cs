using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Shared.Http;

#nullable enable

namespace Piipan.Metrics.Client
{
    public class ParticipantUploadClient : IParticipantUploadReaderApi
    {
        private readonly IAuthorizedApiClient<ParticipantUploadClient> _apiClient;

        public ParticipantUploadClient(IAuthorizedApiClient<ParticipantUploadClient> apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<GetParticipantUploadsResponse> GetLatestUploadsByState()
        {
            return await _apiClient.GetAsync<GetParticipantUploadsResponse>("GetLastUpload");
        }

        public async Task<GetParticipantUploadsResponse> GetUploads(string? state, int perPage, int page)
        {
            return await _apiClient.GetAsync<GetParticipantUploadsResponse>("GetParticipantUploads");
        }
    }
}
