using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    /// <summary>
    /// Represents individual in a Duplicate Participant API response
    /// </summary>
    public class MatchResult
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("matches")]
        public virtual List<MatchResultRecord> Matches { get; set; }
    }
}
