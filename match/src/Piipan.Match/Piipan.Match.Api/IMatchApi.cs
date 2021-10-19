using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Api
{
    public interface IMatchApi
    {
        Task<OrchMatchResponse> FindMatches(OrchMatchRequest request, string initiatingState);
    }
}
