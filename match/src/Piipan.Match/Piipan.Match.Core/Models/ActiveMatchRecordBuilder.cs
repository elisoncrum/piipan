using System.Text.Json;
using Piipan.Match.Orchestrator;

namespace Piipan.Match.Core.Models
{
    public class ActiveMatchRecordBuilder : IActiveMatchRecordBuilder
    {
        private MatchRecordDbo _record = new MatchRecordDbo();

        public ActiveMatchRecordBuilder()
        {
            this.Reset();
        }

        public void Reset()
        {
            this._record = new MatchRecordDbo();
        }

        public IActiveMatchRecordBuilder SetMatch(RequestPerson input, ParticipantRecord match)
        {
            this._record.Input = JsonSerializer.Serialize(input);
            this._record.Data = JsonSerializer.Serialize(match);

            // ldshash is currently the only hash type
            this._record.Hash = input.LdsHash;
            this._record.HashType = "ldshash";

            return this;
        }

        public IActiveMatchRecordBuilder SetStates(string initiatingState, string matchingState)
        {
            this._record.States = new string[] { initiatingState, matchingState };
            this._record.Initiator = initiatingState;
            return this;
        }

        public MatchRecordDbo GetRecord()
        {
            MatchRecordDbo record = this._record;

            this.Reset();

            return record;
        }
    }
}
