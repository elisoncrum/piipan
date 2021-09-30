using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Core.Resolvers
{
    public interface IMatchResolver
    {
        Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request);
    }
}