using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Match.Api;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.Services
{
    public class MatchRecordService : IMatchRecordApi
    {
        private readonly IMatchRecordDao _matchRecordDao;
        private readonly IMatchIdService _matchIdService;
        private readonly ILogger<MatchRecordService> _logger;

        public MatchRecordService(
            IMatchRecordDao matchRecordDao,
            IMatchIdService matchIdService,
            ILogger<MatchRecordService> logger)
        {
            _matchRecordDao = matchRecordDao;
            _matchIdService = matchIdService;
            _logger = logger;
        }

        public async Task<string> AddRecord(IMatchRecord record)
        {
            const int InsertRetries = 10;

            var matchId = _matchIdService.GenerateId();
            var matchRecordDbo = new MatchRecordDbo
            {
                MatchId = matchId,
                Initiator = record.Initiator,
                States = record.States,
                Hash = record.Hash,
                HashType = record.HashType,
                Input = record.Input,
                Data = record.Data,
            };

            // Match IDs are generated randomly from a large pool, but collision
            // is still possible. Retry failed inserts a limited number of times.
            var retries = 0;
            while (retries < InsertRetries)
            {
                try
                {
                    return await _matchRecordDao.AddRecord(matchRecordDbo);
                }
                catch (PostgresException ex)
                {
                    if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        if (retries > 1)
                        {
                            _logger.LogWarning($"{retries} connsecutive match ID collisions.");
                        }


                        matchRecordDbo.MatchId = _matchIdService.GenerateId();
                        retries++;
                    }
                    else
                    {
                        throw (ex);
                    }
                }
            }

            throw new Exception($"Match record table insert failed after {InsertRetries} retries.");
        }
    }
}
