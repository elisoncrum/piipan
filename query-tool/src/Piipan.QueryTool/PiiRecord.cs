using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Piipan.QueryTool
{
    public class PiiRecord
    {
        [Display(Name = "First Name")]
        [JsonPropertyName("first")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [JsonPropertyName("middle")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [JsonPropertyName("last")]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [JsonPropertyName("dob")]
        public string DateOfBirth { get; set; }

        [Display(Name = "SSN")]
        [JsonPropertyName("ssn")]
        public string SocialSecurityNum { get; set; }
    }
}
