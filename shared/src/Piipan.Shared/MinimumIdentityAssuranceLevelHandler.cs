using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Piipan.Shared.Authorization
{
  public class MinimumIdentityAssuranceLevelHandler : AuthorizationHandler<MinimumIdentityAssuranceLevelRequirement>
  {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, 
        MinimumIdentityAssuranceLevelRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == "ial"))
        {
            return Task.CompletedTask;
        }

        var ial = Convert.ToInt32(context.User.FindFirst(c => c.Type == "ial").Value);

        if (ial >= requirement.MinimumIdentityAssuranceLevel)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
  }
}