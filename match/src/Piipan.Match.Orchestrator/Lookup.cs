using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Piipan.Match.Orchestrator
{
    static class Lookup
    {
        const int InsertRetries = 10;
        const string PartitionKey = "lookup";

        /// <summary>
        /// Generate a `lookup_id` based on the provided MatchQuery and save to storage
        /// using the provided ITableStorage instance.
        /// </summary>
        /// <returns>Unique `lookup_id` string for saved query</returns>
        /// <param name="query">MatchQuery instance for saving</param>
        /// <param name="tableStorage">handle to the table storage instance</param>
        /// <param name="log">handle to the function log</param>
        public static async Task<string> Save(MatchQuery query, ITableStorage<QueryEntity> tableStorage, ILogger log)
        {
            var entity = new QueryEntity(PartitionKey, LookupId.Generate());
            entity.Body = query.ToJson();

            // Lookup IDs are generated randomly from a large pool, but collision
            // is still possible. Retry failed inserts a limited number of times.
            var retries = 0;
            while (retries < InsertRetries)
            {
                try
                {
                    var inserted = await tableStorage.InsertAsync(entity);
                    return inserted.RowKey;
                }
                catch (StorageException)
                {
                    if (retries > 1)
                    {
                        log.LogWarning($"{retries} connsecutive lookup ID collisions.");
                    }

                    entity.RowKey = LookupId.Generate();
                    retries++;
                }
            }

            throw new Exception($"Lookup table insert failed after {InsertRetries} retries.");
        }

        /// <summary>
        /// Retrieve a row from storage using a lookup ID
        /// </summary>
        /// <returns>QueryEntity representing the retrieved row</returns>
        /// <param name="lookupId">the unique lookup ID (RowKey) for the row</param>
        /// <param name="tableStorage">handle to the table storage instance</param>
        /// <param name="log">handle to the function log</param>
        public static async Task<MatchQuery> Retrieve(string lookupId, ITableStorage<QueryEntity> tableStorage, ILogger log)
        {
            MatchQuery query = null;

            // Case-insensitive matching
            lookupId = lookupId.ToUpper();

            var row = await tableStorage.PointQueryAsync(PartitionKey, lookupId);

            if (row != null)
            {
                query = JsonConvert.DeserializeObject<MatchQuery>(row.Body);
            }

            return query;
        }
    }

    static class LookupId
    {
        private const string Chars = "23456789BCDFGHJKLMNPQRSTVWXYZ";
        private const int Length = 7;

        /// <summary>
        /// Generate a random alphanumeric ID that conforms to a limited alphabet
        /// limited alphabet and fixed length.
        /// </summary>
        /// <returns>A random 7-character alpha-numeric ID string</returns>
        public static string Generate()
        {
            return Nanoid.Nanoid.Generate(Chars, Length);
        }
    }
}
