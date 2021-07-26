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
            try
            {
                return claimsPrincipal
                    .Claims
                    .Single(c => c.Type == _options.Email)
                    .Value;
            }
            catch (System.InvalidOperationException)
            {
                _logger.LogError($"Unable to extract claim with type {_options.Email} for current user!");
                foreach (var claim in claimsPrincipal.Claims)
                {
                    _logger.LogDebug($"[CLAIM] {claim.Type}: {claim.Value}");
                }
                throw;
            }
        }
    }
}