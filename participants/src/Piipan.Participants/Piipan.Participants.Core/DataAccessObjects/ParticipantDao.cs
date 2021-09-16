using System;
using System.Collections.Generic;
using System.Data;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class ParticipantDao : IParticipantDao
    {
        private readonly IDbConnection _dbConnection;

        public ParticipantDao(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;   
        }

        public IParticipant GetParticipant(string ldsHash, int uploadId)
        {
            throw new NotImplementedException();
        }

        public int AddParticipants(IEnumerable<IParticipant> participants)
        {
            throw new NotImplementedException();
        }
    }
}