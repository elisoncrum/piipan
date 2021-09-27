using System.Threading.Tasks;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.DataAccessObjects
{
    public interface IMatchRecordDao
    {
        Task<string> AddRecord(MatchRecordDbo record);
    }
}
