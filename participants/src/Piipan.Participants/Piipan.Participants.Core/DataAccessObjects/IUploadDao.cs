
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public interface IUploadDao
    {
        Task<IUpload> GetLatestUpload();
        Task<IUpload> AddUpload();
    }
}