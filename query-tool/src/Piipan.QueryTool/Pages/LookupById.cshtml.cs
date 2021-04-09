using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Authentication;

namespace Piipan.QueryTool.Pages
{
    public class LookupByIdModel : PageModel
    {
        private readonly ILogger<LookupByIdModel> _logger;
        private readonly IAuthorizedApiClient _apiClient;
        private readonly OrchestratorApiRequest _apiRequest;

        public LookupByIdModel(ILogger<LookupByIdModel> logger,
                          IAuthorizedApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
            _apiRequest = new OrchestratorApiRequest(_apiClient, _logger);
        }

        [BindProperty]
        public Lookup Query { get; set; }

        public Dictionary<string,object> QueryResult { get; private set; } = new Dictionary<string,object>();
        public List<PiiRecord> Records { get; private set; }

        public String RequestError { get; private set; }
        public bool NoResults = false;

        public async Task<IActionResult> OnPostAsync(Lookup query)
        {
            if (ModelState.IsValid)
            {
                QueryResult = await _apiRequest.SendQuery(
                    Environment.GetEnvironmentVariable("OrchApiUri"),
                    query
                );

                try
                {
                    _logger.LogInformation("Query form submitted");
                    var matches = QueryResult["matches"] as List<PiiRecord>;
                    NoResults = matches.Count == 0;
                    Records = matches;
                    Title = "NAC Query Results";
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
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
