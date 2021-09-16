using System;
using System.Data;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class UploadDao : IUploadDao
    {
        private readonly IDbConnection _dbConnection;

        public UploadDao(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IUpload GetLatestUpload()
        {
            throw new NotImplementedException();
        }

        public void AddUpload()
        {
            throw new NotImplementedException();
        }
    }
}