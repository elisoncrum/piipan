using System;
using System.Collections.Generic;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;

namespace Piipan.Participants.Core.Services
{
    public class ParticipantService : IParticipantApi
    {
        private readonly IParticipantDao _participantDao;
        private readonly IUploadDao _uploadDao;

        public ParticipantService(
            IParticipantDao participantDao,
            IUploadDao uploadDao)
        {
            _participantDao = participantDao;
            _uploadDao = uploadDao;
        }

        public IParticipant GetParticipant(string ldsHash)
        {
            var upload = _uploadDao.GetLatestUpload();
            return _participantDao.GetParticipant(ldsHash, upload.Id);
        }

        public int AddParticipants(IEnumerable<IParticipant> participants)
        {
            return _participantDao.AddParticipants(participants);
        }
    }
}