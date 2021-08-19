using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;

namespace Piipan.Dashboard.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger,
            IClaimsProvider claimsProvider)
            : base(claimsProvider)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
