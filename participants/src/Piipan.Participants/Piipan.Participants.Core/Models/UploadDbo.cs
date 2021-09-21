using System;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.Models
{
    public class UploadDbo : IUpload
    {
        public Int64 Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Publisher { get; set; }
    }
}