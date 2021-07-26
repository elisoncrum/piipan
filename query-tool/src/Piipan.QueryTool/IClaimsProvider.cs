using System.Security.Claims;

namespace Piipan.QueryTool
{
    public interface IClaimsProvider
    {
        string GetEmail(ClaimsPrincipal claimsPrincipal);
    }
}