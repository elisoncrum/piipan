using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Shared;

namespace Piipan.Match.Orchestrator
{
    public class MatchResponse
    {
        [JsonProperty("data")]
        public MatchResponseData Data { get; set; } = new MatchResponseData();
        public string ToJson()
        {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

    public class MatchResponseData
    {
        [JsonProperty("results")]
        public List<StateMatchQueryResponse> Results { get; set; } = new List<StateMatchQueryResponse>();

        [JsonProperty("errors")]
        public List<MatchDataError> Errors { get; set; } = new List<MatchDataError>();
    }

    public class MatchDataError
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }
}
