using System.Threading.Tasks;

#nullable enable

namespace Piipan.Metrics.Api
{
    public interface IParticipantUploadReaderApi
    {
        Task<GetParticipantUploadsResponse> GetLatestUploadsByState();
        Task<GetParticipantUploadsResponse> GetUploads(string? state, int perPage, int page);
    }
}