using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Authentication;

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAuthorizedApiClient _apiClient;
        private readonly OrchestratorApiRequest _apiRequest;

        public IndexModel(ILogger<IndexModel> logger,
                          IAuthorizedApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
            _apiRequest = new OrchestratorApiRequest(_apiClient);
        }

        [BindProperty]
        public PiiRecord Query { get; set; }

        public List<PiiRecord> QueryResult { get; private set; } = new List<PiiRecord>();
        public bool NoResults = false;

        public async Task<IActionResult> OnPostAsync(PiiRecord query)
        {
            if (ModelState.IsValid)
            {
                QueryResult = await _apiRequest.SendQuery(
                    Environment.GetEnvironmentVariable("OrchApiUri"),
                    query
                );

                NoResults = QueryResult.Count == 0;
                Title = "NAC Query Results";
            }
            return Page();
        }

        public string Title { get; private set; } = "";

        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
