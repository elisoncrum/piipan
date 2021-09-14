using System;
using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    /// <summary>
    /// Data Mapper for participant_uploads table in metrics database
    /// </summary>
    public class ParticipantUpload
    {
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("uploaded_at")]
        public DateTime UploadedAt { get; set; }
    }
}
