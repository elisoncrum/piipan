using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    public class Lookup : IQueryable
    {
        [Required]
        [Display(Name = "Lookup ID")]
        [JsonPropertyName("lookup_id")]
        public string LookupId { get; set; }
    }
}
