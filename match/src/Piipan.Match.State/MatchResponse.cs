using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Match.State
{

    public class MatchQueryResponse
    {
        [JsonProperty("matches")]
        public List<PiiRecord> Matches { get; set; }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

    public class PiiRecord
    {
        public PiiRecord()
        {
            StateName = Environment.GetEnvironmentVariable("StateName");
            StateAbbr = Environment.GetEnvironmentVariable("StateAbbr");
        }

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

        [JsonProperty("state_name")]
        public string StateName { get; }

        [JsonProperty("state_abbr")]
        public string StateAbbr { get; }
    }
}
