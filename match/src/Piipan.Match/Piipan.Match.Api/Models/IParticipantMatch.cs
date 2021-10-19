using Piipan.Participants.Api.Models;

namespace Piipan.Match.Api.Models
{
    public interface IParticipantMatch : IParticipant
    {
        string MatchId { get; set; }
    }
}
