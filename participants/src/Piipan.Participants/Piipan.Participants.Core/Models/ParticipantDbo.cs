using System;
using System.Collections.Generic;
using System.Linq;
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
        public Int64 UploadId { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ParticipantDbo p = obj as ParticipantDbo;
            if (p == null)
            {
                return false;
            }

            return 
                LdsHash == p.LdsHash &&
                CaseId == p.CaseId &&
                ParticipantId == p.ParticipantId &&
                BenefitsEndDate.Value.Date == p.BenefitsEndDate.Value.Date &&
                RecentBenefitMonths.SequenceEqual(p.RecentBenefitMonths) &&
                ProtectLocation == p.ProtectLocation &&
                UploadId == p.UploadId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                LdsHash,
                CaseId,
                ParticipantId,
                BenefitsEndDate,
                RecentBenefitMonths,
                ProtectLocation,
                UploadId
            );
        }
    }
}