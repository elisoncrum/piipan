using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    /// <summary>
    /// Represents individual match result in a Duplicate Participant API response
    /// </summary>
    public class MatchResultRecord
    {
        [Required]
        [JsonPropertyName("lds_hash")]
        public string LdsHash { get; set; }

        [Display(Name = "State")]
        [JsonPropertyName("state")]
        public string State { get; set; }

        [Display(Name = "CaseId")]
        [JsonPropertyName("case_id")]
        public string CaseId { get; set; }

        [Display(Name = "ParticipantId")]
        [JsonPropertyName("participant_id")]
        public string ParticipantId { get; set; }

        [Display(Name = "Benefits End Month")]
        [JsonPropertyName("benefits_end_month")]
        public string BenefitsEndMonth { get; set; }

        [Display(Name = "Recent Benefit Months")]
        [JsonPropertyName("recent_benefit_months")]
        public string[] RecentBenefitMonths { get; set; } = new string[0];

        public string RecentBenefitMonthsDisplay
        {
            get { return String.Join(", ", this.RecentBenefitMonths); }
        }

        [Display(Name = "Protect Location")]
        [JsonPropertyName("protect_location")]
        public bool? ProtectLocation { get; set; }

        public string ProtectLocationDisplay
        {
            get
            {
                if (this.ProtectLocation == null) return "Yes";
                return (bool)this.ProtectLocation ? "Yes" : "No";
            }
        }
    }
}
