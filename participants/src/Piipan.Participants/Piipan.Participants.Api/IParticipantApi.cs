using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Api
{
    public interface IParticipantApi
    {
        Task<IEnumerable<IParticipant>> GetParticipants(string ldsHash);
        Task AddParticipants(IEnumerable<IParticipant> participants);
    }
}