using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    public class Lookup : IQueryable
    {
        [Required]
        [Display(Name = "Lookup ID")]
        [JsonPropertyName("lookupId")]
        public string LookupId { get; set; }
    }
}
