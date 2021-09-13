using System;
using System.Collections.Generic;

#nullable enable

namespace Piipan.Metrics.Api
{
    public interface IParticipantUploadApi
    {
        Int64 GetUploadCount(string? state);
        IEnumerable<ParticipantUpload> GetUploads(string? state, int limit, int offset = 0);
        IEnumerable<ParticipantUpload> GetLatestUploadsByState();
        int AddUpload(string state, DateTime uploadedAt);
    }
}