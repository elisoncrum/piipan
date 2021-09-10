using Microsoft.AspNetCore.Http;

namespace Piipan.Metrics.Api.Extensions
{
    public static class IQueryCollectionExtensions
    {
        public static string ParseString(this IQueryCollection query, string key)
        {
            return query[key];
        }

        public static int ParseInt(this IQueryCollection query, string key, int defaultValue = 0)
        {
            int result = defaultValue;
            int.TryParse(query[key], out result);
            return result;
        }
    }
}