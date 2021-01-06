using System;
using Newtonsoft.Json;

namespace Piipan.Match.State
{
    public class MatchQueryRequest
    {
        [JsonProperty("query", Required = Required.Always)]
        public MatchQuery Query { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

    public class MatchQuery
    {
        [JsonProperty("last", Required = Required.Always)]
        public string Last { get; set; }

        [JsonProperty("first", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(NullConverter))]
        public string First { get; set; }

        [JsonProperty("middle", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(NullConverter))]
        public string Middle { get; set; }

        [JsonProperty("dob", Required = Required.Always)]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Dob { get; set; }

        [JsonProperty("ssn", Required = Required.Always)]
        public string Ssn { get; set; }
    }
}
