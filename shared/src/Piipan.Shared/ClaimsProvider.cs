using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Claims
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly ClaimsOptions _options;
        private readonly ILogger<ClaimsProvider> _logger;

        public ClaimsProvider(IOptions<ClaimsOptions> options,
            ILogger<ClaimsProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string GetEmail(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal
                .Claims
                .SingleOrDefault(c => c.Type == _options.Email)?
                .Value;        
        }
    }
}