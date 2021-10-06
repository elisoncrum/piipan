using System;
using System.Linq;
using System.Collections.Generic;
using Piipan.Match.Core.Serializers;
using Piipan.Participants.Api.Models;
using Newtonsoft.Json;

namespace Piipan.Match.Core.Models
{
    public class Participant : IParticipant
    {
        [JsonProperty("lds_hash",
            NullValueHandling = NullValueHandling.Ignore)]
        public string LdsHash { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("case_id")]
        public string CaseId { get; set; }

        [JsonProperty("participant_id")]
        public string ParticipantId { get; set; }

        [JsonProperty("benefits_end_month")]
        [JsonConverter(typeof(JsonConverters.MonthEndConverter))]
        public DateTime? BenefitsEndDate { get; set; }

        [JsonProperty("recent_benefit_months")]
        [JsonConverter(typeof(JsonConverters.MonthEndArrayConverter))]
        public IEnumerable<DateTime> RecentBenefitMonths { get; set; } = new List<DateTime>();

        [JsonProperty("protect_location")]
        public bool? ProtectLocation { get; set; }

        public Participant() {}

        public Participant(IParticipant p) {
            LdsHash = p.LdsHash;
            State = p.State;
            CaseId = p.CaseId;
            ParticipantId = p.ParticipantId;
            BenefitsEndDate = p.BenefitsEndDate;
            RecentBenefitMonths = p.RecentBenefitMonths.ToList();
            ProtectLocation = p.ProtectLocation;
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
