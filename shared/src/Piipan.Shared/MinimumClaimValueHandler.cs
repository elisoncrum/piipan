using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;

namespace Piipan.Shared.Authorization
{
    public class MinimumClaimValueHandler : AuthorizationHandler<MinimumClaimValueRequirement>
    {
        private readonly ILogger<MinimumClaimValueHandler> _logger;
        private readonly IClaimsProvider _claimsProvider;

        public MinimumClaimValueHandler(ILogger<MinimumClaimValueHandler> logger,
            IClaimsProvider claimsProvider)
        {
            _logger = logger;
            _claimsProvider = claimsProvider;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MinimumClaimValueRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == requirement.ClaimType))
            {
                _logger.LogInformation($"Authorization failed: {_claimsProvider.GetEmail(context.User)} is missing {requirement.ClaimType} claim!");
                return Task.CompletedTask;
            }

            var val = context.User.FindFirst(c => c.Type == requirement.ClaimType).Value;
            try
            {
                if (Convert.ToInt32(val) >= requirement.MinimumValue)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogInformation($"Authorization failed: {requirement.ClaimType} for {_claimsProvider.GetEmail(context.User)} ({val}) below minimum!");
                }
            }
            catch (FormatException)
            {
                _logger.LogInformation($"Authorization failed: Unable to convert {requirement.ClaimType} value for {_claimsProvider.GetEmail(context.User)} ({val}) to integer!");
            }
            catch (OverflowException)
            {
                _logger.LogInformation($"Authorization failed: Unable to convert {requirement.ClaimType} value for {_claimsProvider.GetEmail(context.User)} ({val}) to integer!");
            }
            
            return Task.CompletedTask;
        }
    }
}