using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    /// <summary>
    /// Represents body schema of a Duplicate Participant API response
    /// </summary>
    public class MatchResponse
    {
        [JsonPropertyName("data")]
        public MatchResponseData Data { get; set; } = new MatchResponseData();
    }
}
