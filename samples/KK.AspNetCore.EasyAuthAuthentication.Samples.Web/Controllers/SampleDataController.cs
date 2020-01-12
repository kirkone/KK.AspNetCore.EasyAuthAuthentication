namespace KK.AspNetCore.EasyAuthAuthentication.Samples.Web.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        [HttpGet("[action]")]
        [Authorize(Roles = "SystemAdmin")]
        public string UserName()
        {
            _ = this.User.HasClaim(ClaimTypes.Name, "user@somecloud.onmicrosoft.com");
            _ = this.User.HasClaim(ClaimTypes.Role, "SystemAdmin");
            _ = this.HttpContext.User.IsInRole("SystemAdmin");
            _ = this.User.IsInRole("SystemAdmin");
            return this.HttpContext.User.Identity.Name;
        }
    }
}
