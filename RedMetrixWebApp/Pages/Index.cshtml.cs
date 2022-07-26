using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RedMetrixWebApp;

namespace RedMetrixWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly RealtimeChartService _service;

        public RealtimeChart chartdata { get; set; }

        public IndexModel(ILogger<IndexModel> logger, RealtimeChartService service)
        {
            _logger = logger;
            _service=service;
        }

        public void OnGet()
        {
           chartdata  =  _service.GetChartData();
            _logger.LogInformation("Worker running at: {Time} ", chartdata.Updated);
        }

    }
}