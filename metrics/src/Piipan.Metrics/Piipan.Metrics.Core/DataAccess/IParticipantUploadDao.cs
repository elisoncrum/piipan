using System;
using System.Collections.Generic;
using Piipan.Metrics.Api; 

namespace Piipan.Metrics.Core.DataAccess
{
    public interface IParticipantUploadDao
    {
        Int64 GetParticipantUploadCount(string? state);
        IEnumerable<ParticipantUpload> GetParticipantUploads(string? state, int limit, int offset = 0);
    }
}