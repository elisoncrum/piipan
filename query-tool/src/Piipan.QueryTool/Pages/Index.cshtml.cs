using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Authentication;
using Piipan.Shared.Claims;

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAuthorizedApiClient _apiClient;
        private readonly IClaimsProvider _claimsProvider;
        private readonly OrchestratorApiRequest _apiRequest;

        public IndexModel(ILogger<IndexModel> logger,
                          IAuthorizedApiClient apiClient,
                          IClaimsProvider claimsProvider)
        {
            _logger = logger;
            _apiClient = apiClient;
            _claimsProvider = claimsProvider;

            var apiBaseUri = new Uri(Environment.GetEnvironmentVariable("OrchApiUri"));
            _apiRequest = new OrchestratorApiRequest(_apiClient, apiBaseUri, _logger);
        }

        [BindProperty]
        public PiiRecord Query { get; set; }
        public MatchResponse QueryResult { get; private set; }
        public String RequestError { get; private set; }
        public bool NoResults = false;

        public async Task<IActionResult> OnPostAsync()
        {
            Email = _claimsProvider.GetEmail(User);

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Query form submitted");

                    MatchResponse result = await _apiRequest.Match(Query);

                    QueryResult = result;
                    NoResults = QueryResult.matches.Count == 0;
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
        public string Email { get; private set; } = "";

        public void OnGet()
        {
            Title = "NAC Query Tool";
            Email = _claimsProvider.GetEmail(User);
        }
    }
}
