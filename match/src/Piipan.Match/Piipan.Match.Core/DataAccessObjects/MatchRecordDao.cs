using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Match.Core.DataAccessObjects
{
    /// <summary>
    /// Data Access Object for match records
    /// </summary>
    public class MatchRecordDao : IMatchRecordDao
    {
        private readonly IDbConnectionFactory<CollaborationDb> _dbConnectionFactory;
        private readonly ILogger<MatchRecordDao> _logger;

        /// <summary>
        /// Initializes a new instance of MatchRecordDao
        /// </summary>
        public MatchRecordDao(
            IDbConnectionFactory<CollaborationDb> dbConnectionFactory,
            ILogger<MatchRecordDao> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new match record to the database.
        /// </summary>
        /// <remarks>
        /// Throws `DuplicateMatchIdException` if a record with the incoming match ID already exists.
        /// </remarks>
        /// <param name="record">The MatchRecordDbo object that will be added to the database.</param>
        /// <returns>Match ID for inserted record</returns>
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

            // Match IDs are randomly generated at the service level and may result
            // in unique constraint violations in the rare case of a collision.
            try
            {
                return await connection.ExecuteScalarAsync<string>(sql, record);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    throw new DuplicateMatchIdException(
                        "A record with the same Match ID already exists.",
                        ex);
                }

                throw ex;
            }
        }
    }
}
