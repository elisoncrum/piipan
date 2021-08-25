using Piipan.Shared.Claims;

namespace Piipan.QueryTool.Pages
{
    public class SignedOutModel : BasePageModel
    {
        public SignedOutModel(IClaimsProvider claimsProvider)
            : base(claimsProvider)
        {

        }
    }
}