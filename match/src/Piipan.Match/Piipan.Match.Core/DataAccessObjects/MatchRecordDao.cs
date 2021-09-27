using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Match.Core.Models;
using Piipan.Shared;

namespace Piipan.Match.Core.DataAccessObjects
{
    public class MatchRecordDao : IMatchRecordDao
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<MatchRecordDao> _logger;

        public MatchRecordDao(
            IDbConnectionFactory dbConnectionFactory,
            ILogger<MatchRecordDao> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<string> AddRecord(MatchRecordDbo record)
        {
            const string sql = @"
                INSERT INTO matches
                (
                    created_at,
                    match_id,
                    initiator,
                    states,
                    hash,
                    hash_type,
                    input,
                    data
                )
                VALUES
                (
                    now(),
                    @MatchId,
                    @Initiator,
                    @States,
                    @Hash,
                    @HashType::hash_type,
                    @Input::jsonb,
                    @Data::jsonb
                )
                RETURNING match_id;
            ";

            var connection = await _dbConnectionFactory.Build();
            return await connection.ExecuteScalarAsync<string>(sql, record);
        }
    }
}
