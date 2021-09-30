using System.Threading.Tasks;

namespace Piipan.Match.Api
{
    public interface IMatchRecordApi
    {
        Task<string> AddRecord(IMatchRecord record);
    }
}
