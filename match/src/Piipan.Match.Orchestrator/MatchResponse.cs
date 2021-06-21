using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Shared;

namespace Piipan.Match.Orchestrator
{
    public class MatchQueryResponse
    {
        [JsonProperty("data")]
        public List<StateMatchQueryResponse> Data { get; set; } = new List<StateMatchQueryResponse>();

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
