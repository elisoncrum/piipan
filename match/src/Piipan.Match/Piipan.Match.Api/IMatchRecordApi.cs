using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Api
{
    public interface IMatchRecordApi
    {
        Task<string> AddRecord(IMatchRecord record);
    }
}
