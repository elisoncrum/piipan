using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
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
        private readonly IStateService _stateService;
        private readonly ILogger<ParticipantService> _logger;

        public ParticipantService(
            IParticipantDao participantDao,
            IUploadDao uploadDao,
            IStateService stateService,
            ILogger<ParticipantService> logger)
        {
            _participantDao = participantDao;
            _uploadDao = uploadDao;
            _stateService = stateService;
            _logger = logger;
        }

        public async Task<IEnumerable<IParticipant>> GetParticipants(string state, string ldsHash)
        {
            var upload = await _uploadDao.GetLatestUpload();
            return await _participantDao.GetParticipants(state, ldsHash, upload.Id);
        }

        public async Task AddParticipants(IEnumerable<IParticipant> participants)
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

            await _participantDao.AddParticipants(participantDbos);
        }

        public async Task<IEnumerable<string>> GetStates()
        {
            return await _stateService.GetStates();
        }
    }
}