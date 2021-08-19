using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.Shared.Claims;
using Piipan.Shared.Http;
using System;
using System.Linq;

namespace Piipan.Dashboard.Pages
{
    public class BasePageModel : PageModel
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IRequestUrlProvider _requestUrlProvider;

        public BasePageModel(IClaimsProvider claimsProvider, IRequestUrlProvider requestUrlProvider)
        {
            _claimsProvider = claimsProvider;
            _requestUrlProvider = requestUrlProvider;
        }

        public string Email 
        { 
            get { return _claimsProvider.GetEmail(User); }
        }
        public string BaseUrl
        {
            get { return _requestUrlProvider.GetBaseUrl(Request).ToString(); }
        }
    }
}