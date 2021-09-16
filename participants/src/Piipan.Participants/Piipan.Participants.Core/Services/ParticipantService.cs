using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;

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

        public async Task<IParticipant> GetParticipant(string ldsHash)
        {
            var upload = await _uploadDao.GetLatestUpload();
            return await _participantDao.GetParticipant(ldsHash, upload.Id);
        }

        public async Task<int> AddParticipants(IEnumerable<IParticipant> participants)
        {
            var upload = await _uploadDao.AddUpload();

            var participantDbos = participants.Select((p) => 
            {
                return new ParticipantDbo
                {
                    LdsHash = p.LdsHash,
                    CaseId = p.CaseId,
                    ParticipantId = p.ParticipantId,
                    BenefitsEndDate = p.BenefitsEndDate,
                    RecentBenefitMonths = p.RecentBenefitMonths,
                    ProtectLocation = p.ProtectLocation,
                    UploadId = upload.Id
                };
            });

            return await _participantDao.AddParticipants(participantDbos);
        }
    }
}