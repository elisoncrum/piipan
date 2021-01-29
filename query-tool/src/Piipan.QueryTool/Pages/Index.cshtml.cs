using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;


namespace Piipan.QueryTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public string Title { get; private set; }

        [BindProperty]
        public QueryModel Query { get; set; }
        public class QueryModel
        {
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Display(Name = "Middle Name")]
            public string MiddleName { get; set; }
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            [Display(Name = "Date of Birth")]
            public string DateOfBirth { get; set; }
            [Display(Name = "SSN")]
            public string SocialSecurityNum { get; set; }
        }
        public IActionResult OnPost(QueryModel query)
        {
            return Page();
        }
        public void OnGet()
        {
            Title = "NAC Query Tool";
        }
    }
}
