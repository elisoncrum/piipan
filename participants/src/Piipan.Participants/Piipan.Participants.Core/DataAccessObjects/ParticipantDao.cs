using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class ParticipantDao : IParticipantDao
    {
        private readonly IDbConnection _dbConnection;

        public ParticipantDao(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;   
        }

        public Task<ParticipantDbo> GetParticipant(string ldsHash, int uploadId)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddParticipants(IEnumerable<ParticipantDbo> participants)
        {
            throw new NotImplementedException();
        }
    }
}