using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    public class Lookup : IQueryable
    {
        [Required]
        [RegularExpression(@"^[BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz2-9]{7}$",
            ErrorMessage = "Lookup ID must be 7 characters. It can not contain vowels or the numbers 0 or 1.")]
        [Display(Name = "Lookup ID")]
        [JsonPropertyName("lookup_id")]
        public string LookupId { get; set; }
    }
}
