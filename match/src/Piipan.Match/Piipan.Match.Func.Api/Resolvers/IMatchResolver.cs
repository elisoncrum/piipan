using System.Threading.Tasks;
using Piipan.Match.Func.Api.Models;

namespace Piipan.Match.Func.Api.Resolvers
{
    public interface IMatchResolver
    {
        Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request);
    }
}