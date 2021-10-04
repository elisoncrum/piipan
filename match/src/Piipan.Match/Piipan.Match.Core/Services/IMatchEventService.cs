using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Participants.Api.Models;

namespace Piipan.Match.Core.Services
{
    public interface IMatchEventService
    {
        Task ResolveMatchesAsync(RequestPerson person, IEnumerable<IParticipant> matches, string initiatingState);
    }
}
