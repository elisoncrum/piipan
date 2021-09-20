using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    /// <summary>
    /// Represents body data of a Duplicate Participant API response
    /// </summary>
    public class MatchResponseData
    {
        [JsonPropertyName("results")]
        public List<MatchResult> Results { get; set; } = new List<MatchResult>();

        [JsonPropertyName("errors")]
        public List<MatchError> Errors { get; set; } = new List<MatchError>();
    }
}
