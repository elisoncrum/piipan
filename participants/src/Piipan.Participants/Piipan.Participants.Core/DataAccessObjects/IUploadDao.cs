using System;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public interface IUploadDao
    {
        IUpload GetLatestUpload();
        void AddUpload();
    }
}