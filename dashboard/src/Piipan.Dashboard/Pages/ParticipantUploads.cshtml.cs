using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using Piipan.Shared.Claims;

#nullable enable

namespace Piipan.Dashboard.Pages
{
    public class ParticipantUploadsModel : BasePageModel
    {
        private readonly IParticipantUploadReaderApi _participantUploadApi;
        private readonly ILogger<ParticipantUploadsModel> _logger;

        public ParticipantUploadsModel(IParticipantUploadReaderApi participantUploadApi,
            ILogger<ParticipantUploadsModel> logger,
            IClaimsProvider claimsProvider)
            : base(claimsProvider)
        {
            _participantUploadApi = participantUploadApi;
            _logger = logger;
        }
        public string Title = "Most recent upload from each state";
        public List<ParticipantUpload> ParticipantUploadResults { get; private set; } = new List<ParticipantUpload>();
        public string? NextPageParams { get; private set; }
        public string? PrevPageParams { get; private set; }
        public string? StateQuery { get; private set; }
        public static int PerPageDefault = 10;
        public string? RequestError { get; private set; }

        private HttpClient httpClient = new HttpClient();

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading initial results");

                var response = await _participantUploadApi.GetLatestUploadsByState();
                ParticipantUploadResults = response.Data.ToList();
                SetPageLinks(response.Meta);

                // RequestError = null;
                // if (MetricsApiBaseUrl == null)
                // {
                //     throw new Exception("MetricsApiBaseUrl is null.");
                // }
                // var url = MetricsApiBaseUrl + MetricsApiLastUploadPath;
                // var response = await _participantUploadRequest.Get(url);
                // ParticipantUploadResults = response.data;
                // SetPageLinks(response.meta);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "There was an error loading data. You may be able to try again. If the problem persists, please contact system maintainers.";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "Internal Server Error. Please contact system maintainers.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Querying uploads via search form");
                RequestError = null;

                StateQuery = Request.Form["state"];
                var response = await _participantUploadApi.GetUploads(StateQuery, PerPageDefault, 1);
                ParticipantUploadResults = response.Data.ToList();
                SetPageLinks(response.Meta);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "There was an error running your search. You may be able to try again. If the problem persists, please contact system maintainers.";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                RequestError = "Internal Server Error. Please contact system maintainers.";
            }
            return Page();
        }

        private void SetPageLinks(Piipan.Metrics.Api.Meta meta)
        {
            NextPageParams = meta.NextPage;
            PrevPageParams = meta.PrevPage;
        }
    }
}
