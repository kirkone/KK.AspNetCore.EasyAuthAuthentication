namespace KK.AspNetCore.EasyAuthAuthentication.Samples.Web.Pages
{
    using System.Diagnostics;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public ErrorModel(ILogger<ErrorModel> logger) => this.Logger = logger;

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);

        public ILogger<ErrorModel> Logger { get; }

        public void OnGet()
            => this.RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier;
    }
}
