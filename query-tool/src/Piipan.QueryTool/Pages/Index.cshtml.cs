using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Shared.Authentication;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;

namespace Piipan.QueryTool.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAuthorizedApiClient _apiClient;
        private readonly OrchestratorApiRequest _apiRequest;
        private readonly ILdsDeidentifier _ldsDeidentifier;

        public IndexModel(ILogger<IndexModel> logger,
                          IAuthorizedApiClient apiClient,
                          IClaimsProvider claimsProvider,
                          ILdsDeidentifier ldsDeidentifier)
                          : base(claimsProvider)
        {
            _logger = logger;
            _apiClient = apiClient;
            _ldsDeidentifier = ldsDeidentifier;

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
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Query form submitted");

                    string digest = _ldsDeidentifier.Run(
                        Query.LastName,
                        Query.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                        Query.SocialSecurityNum
                    );
                    MatchRequestRecord requestRecord = new MatchRequestRecord() { LdsHash = digest };
                    MatchResponse result = await _apiRequest.Match(requestRecord);
                    QueryResult = result;
                    NoResults = QueryResult.Data.Results.Count == 0 ||
                        QueryResult.Data.Results[0].Matches.Count == 0;
                    Title = "NAC Query Results";
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    if (ex.Message.ToLower().Contains("gregorian"))
                    {
                        RequestError = "Date of birth must be a real date.";
                    }
                    else
                    {
                        RequestError = $"{ex.Message}";
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                    RequestError = "There was an error running your search. Please try again.";
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
