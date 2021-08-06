using Microsoft.AspNetCore.Authorization;

namespace Piipan.Shared.Authorization
{
    public class MinimumClaimValueRequirement : IAuthorizationRequirement
    {
        public string ClaimType { get; }
        public int MinimumValue { get; }
        public MinimumClaimValueRequirement(string claimType, int minimumValue)
        {
            ClaimType = claimType;
            MinimumValue = minimumValue;
        }
    }
}