using System;
using System.Collections.Generic;
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
        public List<PiiRecord> QueryResult { get; private set; } = new List<PiiRecord>();

        public async Task<IActionResult> OnPostAsync(PiiRecord query)
        {
            QueryResult = await _apiRequest.SendQuery(
                "https://ofuncw5wltopux5z7i.azurewebsites.net/api/v1/query",
                query
            );

            Title = "NAC Query Results";
            return Page();
        }

        public string Title { get; private set; } = "";

        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
