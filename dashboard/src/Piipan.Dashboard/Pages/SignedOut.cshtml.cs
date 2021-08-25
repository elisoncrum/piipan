using Piipan.Shared.Claims;

namespace Piipan.Dashboard.Pages
{
    public class SignedOutModel : BasePageModel
    {
        public SignedOutModel(IClaimsProvider claimsProvider)
            : base(claimsProvider)
        {

        }
    }
}