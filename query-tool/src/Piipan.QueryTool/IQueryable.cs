using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    public interface IQueryable
    {
        string LookupId { get; set; }
    }
}
