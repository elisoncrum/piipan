using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.Models;
using Piipan.Participants.Api.Models;

namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// Service layer for resolving match events and match records
    /// </summary>
    public class MatchEventService : IMatchEventService
    {
        private readonly IActiveMatchRecordBuilder _recordBuilder;
        private readonly IMatchRecordApi _recordApi;

        public MatchEventService(
            IActiveMatchRecordBuilder recordBuilder,
            IMatchRecordApi recordApi)
        {
            _recordBuilder = recordBuilder;
            _recordApi = recordApi;
        }

        /// <summary>
        /// Creates a match record for each match found by the match API.
        /// Updates each match to include the resulting `match_id`.
        /// </summary>
        /// <param name="request">The OrchMatchRequest instance derived from the incoming match request</param>
        /// <param name="matchResponse">The OrchMatchResponse instance returned from the match API</param>
        /// <param name="initiatingState">The two-letter postal abbreviation for the state initiating the match request</param>
        /// <returns>The updated `matchResponse` object with `match_id`s</returns>
        public async Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request, OrchMatchResponse matchResponse, string initiatingState)
        {

            matchResponse.Data.Results = (await Task.WhenAll(matchResponse.Data.Results.Select(result =>
                ResolvePersonMatches(
                    request.Data.ElementAt(result.Index),
                    result,
                    initiatingState))))
                .OrderBy(result => result.Index)
                .ToList();

            return matchResponse;
        }

        private async Task<OrchMatchResult> ResolvePersonMatches(RequestPerson person, OrchMatchResult result, string initiatingState)
        {
            // Create a match <-> match record pairing
            var pairs = result.Matches.Select(match =>
                new
                {
                    match,
                    record = _recordBuilder
                                .SetMatch(person, match)
                                .SetStates(initiatingState, match.State)
                                .GetRecord()
                });

            result.Matches = (await Task.WhenAll(
                pairs.Select(pair => ResolveSingleMatch(pair.match, pair.record))));

            return result;
        }

        private async Task<ParticipantMatch> ResolveSingleMatch(IParticipant match, IMatchRecord record)
        {
            var matchId = await _recordApi.AddRecord(record);

            return new ParticipantMatch(match)
            {
                MatchId = matchId
            };
        }
    }
}
