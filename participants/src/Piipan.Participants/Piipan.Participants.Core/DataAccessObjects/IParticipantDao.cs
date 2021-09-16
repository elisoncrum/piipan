using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.DataAccessObjects 
{
    public interface IParticipantDao
    {
        Task<ParticipantDbo> GetParticipant(string ldsHash, int uploadId);
        Task<int> AddParticipants(IEnumerable<ParticipantDbo> participants);
    }
}