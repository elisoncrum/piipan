using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.Dashboard.Api;

#nullable enable

namespace Piipan.Dashboard.Pages
{
    public class ParticipantUploadsModel : PageModel
    {
        public string Title = "Participant Uploads";
        public List<ParticipantUpload> ParticipantUploadResults { get; private set; } = new List<ParticipantUpload>();
        public string? NextPageParams { get; private set; }
        public string? PrevPageParams { get; private set; }
        public string? StateQuery { get; private set; }
        public static int PerPageDefault = 10;
        public static string? BaseUrl = Environment.GetEnvironmentVariable("MetricsApiUri");

        private HttpClient httpClient = new HttpClient();

        public async Task OnGetAsync()
        {
            var url = FormatUrl();
            var api = new ParticipantUploadRequest(httpClient);
            var response = await api.Get(url);
            ParticipantUploadResults = response.data;
            SetPageLinks(response.meta);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (BaseUrl == null)
                throw new Exception("BaseUrl is null.");
            StateQuery = Request.Form["state"];
            var url = $"{BaseUrl}?state={StateQuery}&perPage={PerPageDefault}";
            var api = new ParticipantUploadRequest(httpClient);
            var response = await api.Get(url);
            ParticipantUploadResults = response.data;
            SetPageLinks(response.meta);
            return Page();
        }

        // adds default pagination to the api url if none is present from request params
        private string FormatUrl()
        {
            if (BaseUrl == null)
                throw new Exception("BaseUrl is null.");
            var url = BaseUrl + Request.QueryString;
            StateQuery = Request.Query["state"];
            if (String.IsNullOrEmpty(Request.Query["perPage"]))
            {
                string perPageDefault = $"perPage={PerPageDefault}";
                Regex regex = new Regex(@"\?");
                Match match = regex.Match(url);
                if (match.Success)
                {
                    url += String.Concat("&", perPageDefault);
                }
                else
                {
                    url += String.Concat("?", perPageDefault);
                }
            }
            return url;
        }

        private void SetPageLinks(ParticipantUploadResponseMeta meta)
        {
            NextPageParams = meta.nextPage;
            PrevPageParams = meta.prevPage;
        }
    }
}
