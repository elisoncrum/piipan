using System;
using System.Threading.Tasks;

namespace Piipan.Metrics.Api
{
    public interface IParticipantUploadWriterApi
    {
        Task<int> AddUpload(string state, DateTime uploadedAt);
    }
}