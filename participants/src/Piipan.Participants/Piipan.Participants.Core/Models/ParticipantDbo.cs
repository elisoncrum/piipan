using System;
using System.Collections.Generic;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.Models
{
    public class ParticipantDbo : IParticipant
    {
        public string LdsHash { get; set; }
        public string CaseId { get; set; }
        public string ParticipantId { get; set; }
        public DateTime? BenefitsEndDate { get; set; }
        public IEnumerable<DateTime> RecentBenefitMonths { get; set; }
        public bool? ProtectLocation { get; set; }
        public int UploadId { get; set; }
    }
}