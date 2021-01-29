using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        private readonly ApiRequest _apiRequest = new ApiRequest();

        [BindProperty]
        public PiiRecord Query { get; set; }

        public string QueryResult { get; private set; }
        public IActionResult OnPost(PiiRecord query)
        {
            // QueryResult = _apiRequest.QueryOrchestrator();
            return Page();
        }

        public string Title { get; private set; }

        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
