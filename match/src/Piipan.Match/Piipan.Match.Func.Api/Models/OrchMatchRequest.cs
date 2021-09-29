using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Shared;

namespace Piipan.Match.Func.Api.Models
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

    /// <summary>
    /// Represents each person in an API request using PII
    /// </summary>
    public class RequestPersonWithPii
    {
        [JsonProperty("last", Required = Required.Always)]
        public string Last { get; set; }

        [JsonProperty("first", Required = Required.Always)]
        public string First { get; set; }

        [JsonProperty("middle", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonConverters.NullConverter))]
        public string Middle { get; set; }

        [JsonProperty("dob", Required = Required.Always)]
        [JsonConverter(typeof(JsonConverters.DateTimeConverter))]
        public DateTime Dob { get; set; }

        [JsonProperty("ssn", Required = Required.Always)]
        public string Ssn { get; set; }
    }
}
