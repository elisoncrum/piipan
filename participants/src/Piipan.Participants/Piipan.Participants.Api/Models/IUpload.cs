using System;

namespace Piipan.Participants.Api.Models
{
    public interface IUpload
    {
        int Id { get; set; }
        DateTime CreatedAt { get; set; }
        string Publisher { get; set; }
    }
}