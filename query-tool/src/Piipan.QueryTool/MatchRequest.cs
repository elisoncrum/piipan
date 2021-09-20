using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    /// <summary>
    /// Represents entire body schema of a Duplicate Participant API request
    /// </summary>
    public class MatchRequest
    {
        [JsonPropertyName("data")]
        public List<MatchRequestRecord> Data { get; set; }
    }
}
