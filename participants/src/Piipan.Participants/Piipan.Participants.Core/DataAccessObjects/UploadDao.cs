using System.Threading.Tasks;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.Models;
using Dapper;
using Piipan.Shared;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class UploadDao : IUploadDao
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UploadDao(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IUpload> GetLatestUpload()
        {
            var connection = await _dbConnectionFactory.Build();
            return await connection
                .QuerySingleAsync<UploadDbo>(@"
                    SELECT id, created_at, publisher
                    FROM uploads
                    ORDER BY id DESC
                    LIMIT 1");
        }

        public async Task<IUpload> AddUpload()
        {
            var connection = await _dbConnectionFactory.Build();
            var tx = connection.BeginTransaction();

            await connection.ExecuteAsync(@"
                INSERT INTO uploads (created_at, publisher)
                VALUES (now(), current_user)");

            var upload = await connection.QuerySingleAsync<UploadDbo>(@"
                    SELECT id, created_at, publisher
                    FROM uploads
                    ORDER BY id DESC
                    LIMIT 1");

            tx.Commit();

            return upload;
        }
    }
}