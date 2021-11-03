using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Piipan.Dashboard.Api;
using Piipan.Shared.Claims;

#nullable enable

namespace Piipan.Dashboard.Pages
{
    public class ParticipantUploadsModel : BasePageModel
    {
        private readonly IParticipantUploadRequest _participantUploadRequest;
        private readonly ILogger<ParticipantUploadsModel> _logger;

        public ParticipantUploadsModel(IParticipantUploadRequest participantUploadRequest,
            ILogger<ParticipantUploadsModel> logger,
            IClaimsProvider claimsProvider)
            : base(claimsProvider)
        {
            _participantUploadRequest = participantUploadRequest;
            _logger = logger;
        }
        public string Title = "Most recent upload from each state";
        public List<ParticipantUpload> ParticipantUploadResults { get; private set; } = new List<ParticipantUpload>();
        public string? NextPageParams { get; private set; }
        public string? PrevPageParams { get; private set; }
        public string? StateQuery { get; private set; }
        public static int PerPageDefault = 10;
        public static string ApiUrlKey = "MetricsApiUri";
        public string? MetricsApiBaseUrl = Environment.GetEnvironmentVariable(ApiUrlKey);
        public string MetricsApiSearchPath = "/getparticipantuploads";
        public string MetricsApiLastUploadPath = "/getlastupload";
        public string? RequestError { get; private set; }

        private HttpClient httpClient = new HttpClient();

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading initial results");
                RequestError = null;
                if (MetricsApiBaseUrl == null)
                {
                    throw new Exception("MetricsApiBaseUrl is null.");
                }
                var url = MetricsApiBaseUrl + MetricsApiLastUploadPath;
                var response = await _participantUploadRequest.Get(url);
                ParticipantUploadResults = response.data;
                SetPageLinks(response.meta);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "There was an error running your search. Please try again.";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "Internal Server Error. Please contact support.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Querying uploads via search form");

                if (MetricsApiBaseUrl == null)
                {
                    throw new Exception("MetricsApiBaseUrl is null.");
                }

                StateQuery = Request.Form["state"];
                var url = QueryHelpers.AddQueryString(MetricsApiBaseUrl + MetricsApiSearchPath, "state", StateQuery);
                url = QueryHelpers.AddQueryString(url, "perPage", PerPageDefault.ToString());
                var response = await _participantUploadRequest.Get(url);
                ParticipantUploadResults = response.data;
                SetPageLinks(response.meta);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
            }
            return Page();
        }

        // adds default pagination to the api url if none is present from request params
        private string FormatUrl()
        {
            if (MetricsApiBaseUrl == null)
            {
                throw new Exception("MetricsApiBaseUrl is null.");
            }
            var url = MetricsApiBaseUrl + Request.QueryString;
            StateQuery = Request.Query["state"];
            if (String.IsNullOrEmpty(Request.Query["perPage"]))
                url = QueryHelpers.AddQueryString(url, "perPage", PerPageDefault.ToString());
            return url;
        }

        private void SetPageLinks(ParticipantUploadResponseMeta meta)
        {
            NextPageParams = meta.nextPage;
            PrevPageParams = meta.prevPage;
        }
    }
}
