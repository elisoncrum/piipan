using System.Collections.Generic;
using System.Security.Claims;

namespace Piipan.Shared.Authorization
{
    public class RequiredClaim
    {
        public string Type { get; set; }
        public IEnumerable<string> Values { get; set; }
    }

    public class AuthorizationPolicyOptions
    {
        public const string SectionName = "AuthorizationPolicy";
        
        public IEnumerable<RequiredClaim> RequiredClaims { get; set; }
    }
}