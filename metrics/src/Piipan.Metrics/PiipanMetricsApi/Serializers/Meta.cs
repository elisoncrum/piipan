using System;

#nullable enable
namespace Piipan.Metrics.Api
{
    namespace Serializers
    {
        public class Meta
        {
            public int page { get; set; }
            public int perPage { get; set; }
            public Int64 total { get; set; }
            public string? nextPage { get; set; }
            public string? prevPage { get; set; }
        }
    }
}
