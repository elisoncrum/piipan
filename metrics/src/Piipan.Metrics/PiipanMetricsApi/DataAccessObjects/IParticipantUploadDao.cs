using System;
using System.Collections.Generic;
using Piipan.Metrics.Models;

#nullable enable

namespace Piipan.Metrics.Api.DataAccessObjects
{
    public interface IParticipantUploadDao
    {
        Int64 GetParticipantUploadCount(string? state);
        IEnumerable<ParticipantUpload> GetParticipantUploadsForState(string? state, int limit, int offset);
    }
}