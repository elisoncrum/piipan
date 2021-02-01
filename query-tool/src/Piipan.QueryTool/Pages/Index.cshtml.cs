using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public PiiRecord Query { get; set; }

        private readonly OrchestratorApiRequest _apiRequest = new OrchestratorApiRequest();
        public string QueryResult { get; private set; } = "";

        public async Task<IActionResult> OnPostAsync(PiiRecord query)
        {
            QueryResult = await _apiRequest.SendQuery(
                "https://234ad987-27d2-4ea6-8d7f-7743c7695c5a.mock.pstmn.io/query",
                query
            );
            Title = "NAC Query Results";
            return Page();
        }

        public string Title { get; private set; }

        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
