using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    public class PiiRecord : IQueryable
    {
        [Required]
        [Display(Name = "First Name")]
        [JsonPropertyName("first")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [JsonPropertyName("middle")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [JsonPropertyName("last")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date),
            DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        [JsonPropertyName("dob")]
        public DateTime? DateOfBirth { get; set; }

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

        [Display(Name = "Lookup ID")]
        [JsonPropertyName("lookupId")]
        public string LookupId {get; set;}
    }
}
