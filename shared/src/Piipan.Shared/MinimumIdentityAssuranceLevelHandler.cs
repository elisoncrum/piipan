using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;

namespace Piipan.Shared.Authorization
{
    public class MinimumIdentityAssuranceLevelHandler : AuthorizationHandler<MinimumIdentityAssuranceLevelRequirement>
    {
        private readonly ILogger<MinimumIdentityAssuranceLevelHandler> _logger;
        private readonly IClaimsProvider _claimsProvider;

        public MinimumIdentityAssuranceLevelHandler(ILogger<MinimumIdentityAssuranceLevelHandler> logger,
            IClaimsProvider claimsProvider)
        {
            _logger = logger;
            _claimsProvider = claimsProvider;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MinimumIdentityAssuranceLevelRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "ial"))
            {
                _logger.LogInformation($"Authorization failed: {_claimsProvider.GetEmail(context.User)} is missing ial claim!");
                return Task.CompletedTask;
            }

            var ial = context.User.FindFirst(c => c.Type == "ial").Value;
            try
            {
                if (Convert.ToInt32(ial) >= requirement.MinimumIdentityAssuranceLevel)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogInformation($"Authorization failed: IAL for {_claimsProvider.GetEmail(context.User)} ({ial}) below minimum!");
                }
            }
            catch (FormatException)
            {
                _logger.LogInformation($"Authorization failed: Unable to convert IAL value for {_claimsProvider.GetEmail(context.User)} ({ial}) to integer!");
            }
            catch (OverflowException)
            {
                _logger.LogInformation($"Authorization failed: Unable to convert IAL value for {_claimsProvider.GetEmail(context.User)} ({ial}) to integer!");
            }
            
            return Task.CompletedTask;
        }
    }
}