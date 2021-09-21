using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    /// <summary>
    /// Represents a Duplicate Participant API request data item
    /// </summary>
    public class MatchRequestRecord
    {
        [Required]
        [JsonPropertyName("lds_hash")]
        public string LdsHash { get; set; }
    }
}
