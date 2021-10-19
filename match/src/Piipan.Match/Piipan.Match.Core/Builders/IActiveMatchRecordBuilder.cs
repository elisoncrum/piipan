using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Participants.Api.Models;

namespace Piipan.Match.Core.Builders
{
    public interface IActiveMatchRecordBuilder
    {
        IActiveMatchRecordBuilder SetMatch(RequestPerson input, IParticipant match);
        IActiveMatchRecordBuilder SetStates(string initiatingState, string matchingState);

        IMatchRecord GetRecord();
    }
}
