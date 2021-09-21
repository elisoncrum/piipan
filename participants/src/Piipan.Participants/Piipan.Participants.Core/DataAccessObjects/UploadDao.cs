using System.Data;
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.Models;
using Dapper;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class UploadDao : IUploadDao
    {
        private readonly IDbConnection _dbConnection;

        public UploadDao(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IUpload> GetLatestUpload()
        {
            return await _dbConnection.QuerySingleAsync<UploadDbo>(@"
                SELECT id, created_at, publisher
                FROM uploads
                ORDER BY id DESC
                LIMIT 1");
        }

        public async Task<IUpload> AddUpload()
        {
            var tx = _dbConnection.BeginTransaction();

            await _dbConnection.ExecuteAsync(@"
                INSERT INTO uploads (created_at, publisher)
                VALUES (now(), current_user)");

            var upload = await GetLatestUpload();

            tx.Commit();

            return upload;
        }
    }
}