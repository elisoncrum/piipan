using System;
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
            var apiBaseUri = new Uri(Environment.GetEnvironmentVariable("OrchApiUri"));
            _apiRequest = new OrchestratorApiRequest(_apiClient, apiBaseUri, _logger);
        }

        [BindProperty]
        public Lookup Query { get; set; }
        public LookupResponse Record { get; set; }
        public String RequestError { get; private set; }
        public bool NoResults = false;

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Lookup form submitted");

                    LookupResponse result = await _apiRequest.Lookup(Query.LookupId);

                    Record = result;
                    NoResults = (Record == null || Record.data == null);
                    Title = "NAC Lookup Results";
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
