using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.DataAccessObjects 
{
    public interface IParticipantDao
    {
        Task<IEnumerable<ParticipantDbo>> GetParticipants(string ldsHash, Int64 uploadId);
        Task AddParticipants(IEnumerable<ParticipantDbo> participants);
    }
}