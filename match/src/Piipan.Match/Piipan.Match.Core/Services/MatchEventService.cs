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
        /// Creates a match record for each person-level match
        /// </summary>
        /// <param name="person">The matching RequestPerson instance from the match request</param>
        /// <param name="matches">The collection of one or more matches returned by the match request</param>
        /// <param name="initiatingState">The two-letter postal abbreviation for the state initiating the match request</param>
        public async Task ResolveMatchesAsync(RequestPerson person, IEnumerable<IParticipant> matches, string initiatingState)
        {
            var records = matches.Select(match =>
                _recordBuilder
                    .SetMatch(person, match)
                    .SetStates(initiatingState, match.State)
                    .GetRecord());
            await Task.WhenAll(records.Select(r => _recordApi.AddRecord(r)));
        }
    }
}
