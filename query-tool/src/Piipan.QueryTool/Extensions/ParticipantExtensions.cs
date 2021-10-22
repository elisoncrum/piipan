using System;
using System.Linq;
using Piipan.Participants.Api.Models;

namespace Piipan.QueryTool.Extensions
{
    public static class ParticipantExtensions
    {
        public static string BenefitsEndDateDisplay(this IParticipant participant)
        {
            return participant.BenefitsEndDate?.ToString("yyyy-MM");
        }

        public static string RecentBenefitMonthsDisplay(this IParticipant participant)
        {
            return String.Join(", ", participant.RecentBenefitMonths.Select(dt => dt.ToString("yyyy-MM")));
        }

        public static string ProtectLocationDisplay(this IParticipant participant)
        {
            if (participant.ProtectLocation == null)
            {
                return "Yes";
            }
            return participant.ProtectLocation.Value ? "Yes" : "No";
        }
    }
}