using System;
using System.Collections.Generic;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.DataAccessObjects 
{
    public interface IParticipantDao
    {
        IParticipant GetParticipant(string ldsHash, int uploadId);
        int AddParticipants(IEnumerable<IParticipant> participants);
    }
}