using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
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
        /// Creates a match record for each match found by the match API
        /// </summary>
        /// <param name="request">The OrchMatchRequest instance derived from the incoming match request</param>
        /// <param name="matchResponse">The OrchMatchResponse instance returned from the match API</param>
        /// <param name="initiatingState">The two-letter postal abbreviation for the state initiating the match request</param>
        public async Task ResolveMatches(OrchMatchRequest request, OrchMatchResponse matchResponse, string initiatingState)
        {

            await Task.WhenAll(
                matchResponse.Data.Results.Select(result =>
                    ResolvePersonMatches(
                        request.Data.ElementAt(result.Index),
                        result,
                        initiatingState)));
        }

        private async Task ResolvePersonMatches(RequestPerson person, OrchMatchResult result, string initiatingState)
        {
            var records = result.Matches.Select(match =>
                _recordBuilder
                    .SetMatch(person, match)
                    .SetStates(initiatingState, match.State)
                    .GetRecord());
            await Task.WhenAll(records.Select(r => _recordApi.AddRecord(r)));
        }
    }
}
