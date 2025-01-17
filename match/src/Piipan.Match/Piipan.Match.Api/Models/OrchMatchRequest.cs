using System.Collections.Generic;
using Newtonsoft.Json;

namespace Piipan.Match.Api.Models
{
    /// <summary>
    /// Represents the full API request from a client when using de-identified data
    /// </summary>
    public class OrchMatchRequest
    {
        [JsonProperty("data", Required = Required.Always)]
        public List<RequestPerson> Data { get; set; } = new List<RequestPerson>();
    }

    /// <summary>
    /// Represents each person in an API request using de-identified data
    /// </summary>
    public class RequestPerson
    {
        [JsonProperty("lds_hash",
            Required = Required.Always,
            NullValueHandling = NullValueHandling.Ignore)]
        public string LdsHash { get; set; }
    }
}
