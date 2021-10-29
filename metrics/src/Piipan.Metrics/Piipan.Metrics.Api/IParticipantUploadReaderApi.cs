using System;
using System.Threading.Tasks;

#nullable enable

namespace Piipan.Metrics.Api
{
    public interface IParticipantUploadReaderApi
    {
        Task<Int64> GetUploadCount(string? state);
        //IEnumerable<ParticipantUpload> GetUploads(string? state, int limit, int offset = 0);
        Task<GetParticipantUploadsResponse> GetLatestUploadsByState();
        Task<GetParticipantUploadsResponse> GetUploads(string? state, int perPage, int page);
    }
}