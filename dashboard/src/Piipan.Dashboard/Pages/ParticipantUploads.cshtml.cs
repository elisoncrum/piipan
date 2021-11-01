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

        private HttpClient httpClient = new HttpClient();

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading initial results");

                var response = await _participantUploadApi.GetLatestUploadsByState();
                ParticipantUploadResults = response.Data.ToList();
                SetPageLinks(response.Meta);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Querying uploads via search form");

                StateQuery = Request.Form["state"];
                var response = await _participantUploadApi.GetUploads(StateQuery, PerPageDefault, 1);
                ParticipantUploadResults = response.Data.ToList();
                SetPageLinks(response.Meta);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
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
