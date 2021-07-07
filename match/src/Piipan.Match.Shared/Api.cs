using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Piipan.Match.Shared
{
    public class ApiErrorResponse
    {
        [JsonProperty("errors", Required = Required.Always)]
        public List<ApiHttpError> Errors { get; set; } = new List<ApiHttpError>();
    }

    public class ApiHttpError
    {
        [JsonProperty("status_code")]
        public System.Net.HttpStatusCode StatusCode { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }
}
