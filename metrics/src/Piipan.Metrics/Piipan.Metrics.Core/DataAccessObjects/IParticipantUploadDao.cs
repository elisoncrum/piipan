using System;
using System.Collections.Generic;
using Piipan.Metrics.Api; 

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public interface IParticipantUploadDao
    {
        Int64 GetUploadCount(string? state);
        IEnumerable<ParticipantUpload> GetUploads(string? state, int limit, int offset = 0);
        IEnumerable<ParticipantUpload> GetLatestUploadsByState();
        int AddUpload(string state, DateTime uploadedAt);
    }
}