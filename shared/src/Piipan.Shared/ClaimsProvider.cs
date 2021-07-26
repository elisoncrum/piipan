using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Claims
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly ClaimsOptions _options;

        public ClaimsProvider(IOptions<ClaimsOptions> options)
        {
            _options = options.Value;
        }

        public string GetEmail(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal
                .Claims
                .Single(c => c.Type == _options.Email)
                .Value;
        }
    }
}