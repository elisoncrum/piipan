using Microsoft.AspNetCore.Authorization;

namespace Piipan.Shared.Authorization
{
    public class MinimumIdentityAssuranceLevelRequirement : IAuthorizationRequirement
    {
        public int MinimumIdentityAssuranceLevel { get; }

        public MinimumIdentityAssuranceLevelRequirement(int minimumIdentityAssuranceLevel)
        {
            MinimumIdentityAssuranceLevel = minimumIdentityAssuranceLevel;
        }
    }
}