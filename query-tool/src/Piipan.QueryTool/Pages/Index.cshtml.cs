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
        public String RequestError { get; private set; }
        public bool NoResults = false;

        public async Task<IActionResult> OnPostAsync(PiiRecord query)
        {
            if (ModelState.IsValid)
            {
                QueryResult = await _apiRequest.SendQuery(
                    Environment.GetEnvironmentVariable("OrchApiUri"),
                    query
                );

                try
                {
                    NoResults = QueryResult.Count == 0;
                    Title = "NAC Query Results";
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    RequestError = "There was an error running your search";
                }
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
