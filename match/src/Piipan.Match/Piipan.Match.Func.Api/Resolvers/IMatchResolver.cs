using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Match.Func.Api.Resolvers
{
    public interface IMatchResolver
    {
        Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request);
    }
}