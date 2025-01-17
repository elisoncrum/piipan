using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// Service layer for interacting with match records.
    /// </summary>
    public class MatchRecordService : IMatchRecordApi
    {
        private readonly IMatchRecordDao _matchRecordDao;
        private readonly IMatchIdService _matchIdService;
        private readonly ILogger<MatchRecordService> _logger;

        /// <summary>
        /// Initializes a new instance of MatchRecordService
        /// </summary>
        public MatchRecordService(
            IMatchRecordDao matchRecordDao,
            IMatchIdService matchIdService,
            ILogger<MatchRecordService> logger)
        {
            _matchRecordDao = matchRecordDao;
            _matchIdService = matchIdService;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new match record to the underlying datastore.
        /// </summary>
        /// <remarks>
        /// Throws `InvalidOperationException` in the rare case of repeated match ID collisions.
        /// </remarks>
        /// <param name="record">The match record object that will be added to the datastore.</param>
        /// <returns>Match ID for inserted record</returns>
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
                catch (DuplicateMatchIdException)
                {
                    if (retries > 1)
                    {
                        _logger.LogWarning($"{retries} connsecutive match ID collisions.");
                    }

                    matchRecordDbo.MatchId = _matchIdService.GenerateId();
                    retries++;
                }
            }

            throw new InvalidOperationException(
                $"Match record table insert failed after {InsertRetries} retries.");
        }

        public async Task<IEnumerable<IMatchRecord>> GetRecords(IMatchRecord record)
        {
            return await _matchRecordDao.GetRecords((MatchRecordDbo)record);
        }
    }
}
