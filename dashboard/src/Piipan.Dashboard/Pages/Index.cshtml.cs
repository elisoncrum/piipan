using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        public string Message { get; private set; } = "Hello";
        public string Email { get; private set; } = "";

        public void OnGet()
        {
            Message += ", world.";
            Email = _claimsProvider.GetEmail(User);
        }
    }
}
