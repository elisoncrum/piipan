using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Piipan.Dashboard.Pages
{
    public class ParticipantUploadsModel : PageModel
    {

        public string Title = "Participant Uploads";

        public List<ParticipantUpload> ParticipantUploadResults { get; private set; } = new List<ParticipantUpload>();
        public string NextPageParams { get; private set; }
        public string PrevPageParams { get; private set; }

        public string StateQuery { get; private set; }
        private static int PerPageDefault = 10;
        private static string BaseUrl = "https://piipanmetricsapiztqzsbh432oyw.azurewebsites.net/api/GetParticipantUploads";

        public async Task OnGetAsync()
        {
            var url = BaseUrl + Request.QueryString;
            if (!String.IsNullOrEmpty(Request.Query["state"]))
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
            var response = await ApiRequest.Get(url);
            ParticipantUploadResults = response.data;
            SetPageLinks(response.meta);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            StateQuery = Request.Form["state"];
            var url = $"{BaseUrl}?state={StateQuery}&perPage={PerPageDefault}";
            var response = await ApiRequest.Get(url);
            ParticipantUploadResults = response.data;
            SetPageLinks(response.meta);
            return Page();
        }

        private void SetPageLinks(Meta meta)
        {
            if (!String.IsNullOrEmpty(meta.nextPage))
                NextPageParams = meta.nextPage;
            if (!String.IsNullOrEmpty(meta.prevPage))
                PrevPageParams = meta.prevPage;
        }
    }
}
