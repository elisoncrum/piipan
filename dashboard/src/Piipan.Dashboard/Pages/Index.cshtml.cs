using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;
using Piipan.Shared.Http;
using System.Linq;

namespace Piipan.Dashboard.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger,
            IClaimsProvider claimsProvider,
            IRequestUrlProvider requestUrlProvider) 
            : base(claimsProvider, requestUrlProvider)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
