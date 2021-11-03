using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Metrics.Api; 

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public interface IParticipantUploadDao
    {
        Task<Int64> GetUploadCount(string? state);
        Task<IEnumerable<ParticipantUpload>> GetUploads(string? state, int limit, int offset = 0);
        Task<IEnumerable<ParticipantUpload>> GetLatestUploadsByState();
        Task<int> AddUpload(string state, DateTime uploadedAt);
    }
}