using System;
using System.Collections.Generic;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Api
{
    public interface IParticipantApi
    {
        IParticipant GetParticipant(string ldsHash);
        int AddParticipants(IEnumerable<IParticipant> participants);
    }
}