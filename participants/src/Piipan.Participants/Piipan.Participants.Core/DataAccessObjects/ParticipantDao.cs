using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Participants.Core.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Shared;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class ParticipantDao : IParticipantDao
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<ParticipantDao> _logger;

        public ParticipantDao(
            IDbConnectionFactory dbConnectionFactory,
            ILogger<ParticipantDao> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<ParticipantDbo>> GetParticipants(string ldsHash, Int64 uploadId)
        {
            return await _dbConnectionFactory
                .Build()
                .QueryAsync<ParticipantDbo>(@"
                    SELECT 
                        lds_hash LdsHash,
                        participant_id ParticipantId,
                        case_id CaseId,
                        benefits_end_date BenefitsEndDate,
                        recent_benefit_months RecentBenefitMonths,
                        protect_location ProtectLocation,
                        upload_id UploadId
                    FROM participants
                    WHERE lds_hash=@ldsHash
                        AND upload_id=@uploadId",
                    new
                    {
                        ldsHash = ldsHash,
                        uploadId = uploadId
                    }
                );
        }

        public async Task AddParticipants(IEnumerable<ParticipantDbo> participants)
        {
            const string sql = @"
                INSERT INTO participants
                (
                    lds_hash,
                    upload_id,
                    case_id,
                    participant_id,
                    benefits_end_date,
                    recent_benefit_months,
                    protect_location
                )
                VALUES
                (
                    @LdsHash,
                    @UploadId,
                    @CaseId,
                    @ParticipantId,
                    @BenefitsEndDate,
                    @RecentBenefitMonths::date[],
                    @ProtectLocation
                )
            ";

            foreach (var participant in participants)
            {
                _logger.LogDebug(
                    $"Adding participant for upload {participant.UploadId} with LDS Hash: {participant.LdsHash}");

                await _dbConnectionFactory
                    .Build()
                    .ExecuteAsync(sql, participant);
            }
        }
    }
}