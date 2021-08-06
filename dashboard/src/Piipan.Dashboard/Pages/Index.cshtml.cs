using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Claims;

namespace Piipan.Dashboard.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IClaimsProvider _claimsProvider;

        public IndexModel(ILogger<IndexModel> logger,
            IClaimsProvider claimsProvider)
        {
            _logger = logger;
            _claimsProvider = claimsProvider;
        }
        public string Email { get; private set; } = "";

        public void OnGet()
        {
            Email = _claimsProvider.GetEmail(User);
        }
    }
}
