using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Piipan.QueryTool
{
    public class PiiRecord
    {
        [Required]
        [Display(Name = "First Name")]
        [JsonPropertyName("first")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Middle Name")]
        [JsonPropertyName("middle")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [JsonPropertyName("last")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$",
            ErrorMessage = "Birth date must have the form YYYY-MM-DD")]
        [Display(Name = "Date of Birth")]
        [JsonPropertyName("dob")]
        public string DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}-\d{2}-\d{4}$",
            ErrorMessage = "SSN must have the form 000-00-0000")]
        [Display(Name = "SSN")]
        [JsonPropertyName("ssn")]
        public string SocialSecurityNum { get; set; }

        [Display(Name = "StateName")]
        [JsonPropertyName("state_name")]
        public string StateName { get; set; }

        [Display(Name = "StateAbbr")]
        [JsonPropertyName("state_abbr")]
        public string StateAbbr { get; set; }
    }
}
