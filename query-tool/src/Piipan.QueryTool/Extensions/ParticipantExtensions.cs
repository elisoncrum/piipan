using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.QueryTool.Extensions
{
    public static class ParticipantExtensions
    {
        public static string RecentBenefitMonthsDisplay(this IParticipant participant)
        {
            return String.Join(", ", participant.RecentBenefitMonths);
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