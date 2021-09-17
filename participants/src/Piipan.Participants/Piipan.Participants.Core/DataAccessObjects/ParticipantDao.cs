using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Piipan.Participants.Core.Models;
using Dapper;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class ParticipantDao : IParticipantDao
    {
        private readonly IDbConnection _dbConnection;

        public ParticipantDao(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;   
        }

        public async Task<ParticipantDbo> GetParticipant(string ldsHash, int uploadId)
        {
            return await _dbConnection.QuerySingleAsync<ParticipantDbo>(@"
                SELECT participant_id ParticipantId,
                    case_id CaseId,
                    benefits_end_date BenefitsEndDate,
                    recent_benefit_months RecentBenefitMonths,
                    protect_location ProtectLocation
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
                    @RecentBenefitMonths,
                    @ProtectLocation
                )
            ";

            foreach (var participant in participants)
            {
                await _dbConnection.ExecuteAsync(sql, participant);
            }
        }
    }
}