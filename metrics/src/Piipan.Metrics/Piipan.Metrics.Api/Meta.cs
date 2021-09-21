using System;
using Newtonsoft.Json;

#nullable enable

namespace Piipan.Metrics.Api
{
    public class Meta
    {
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("perPage")]
        public int PerPage { get; set; }
        [JsonProperty("total")]
        public Int64 Total { get; set; }
        [JsonProperty("nextPage")]
        public string? NextPage { get; set; }
        [JsonProperty("prevPage")]
        public string? PrevPage { get; set; }
    }
}