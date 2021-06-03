using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Match.Orchestrator
{

    public class MatchQueryResponse
    {
        [JsonProperty("lookup_id")]
        public string LookupId { get; set; }

        [JsonProperty("matches")]
        public List<PiiRecord> Matches { get; set; }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

    public class PiiRecord
    {
        [JsonProperty("last")]
        public string Last { get; set; }

        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("middle")]
        public string Middle { get; set; }

        [JsonProperty("ssn")]
        public string Ssn { get; set; }

        [JsonProperty("dob")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Dob { get; set; }

        [JsonProperty("exception")]
        public string Exception { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        // Deprecated, duplicates value of `State`
        [JsonProperty("state_abbr")]
        public string StateAbbr { get; set; }

        [JsonProperty("case_id")]
        public string CaseId { get; set; }

        [JsonProperty("participant_id")]
        public string ParticipantId { get; set; }

        [JsonProperty("benefits_end_month")]
        [JsonConverter(typeof(DateMonthConverter))]
        public DateTime? BenefitsEndMonth { get; set; }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
