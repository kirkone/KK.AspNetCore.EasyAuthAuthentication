namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System.Security.Claims;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class EasyAuthFacebookService : EasyAuthWithHeaderService<EasyAuthFacebookService>, IEasyAuthAuthentificationService
    {
        private readonly ILogger<EasyAuthFacebookService> logger;
        public EasyAuthFacebookService(ILogger<EasyAuthFacebookService> logger) : base(logger){
            this.logger = logger;
            this.defaultOptions = new ProviderOptions(typeof(EasyAuthFacebookService).Name, "name", ClaimTypes.Role);
        }

        public new AuthenticateResult AuthUser(HttpContext context)
        {
            this.logger.LogInformation("Try authentification with facebook account.");
            return base.AuthUser(context);
        }

        public new AuthenticateResult AuthUser(HttpContext context, ProviderOptions options)
        {
            this.logger.LogInformation("Try authentification with facebook account.");
            return base.AuthUser(context, options);
        }

        public new bool CanHandleAuthentification(HttpContext httpContext) => base.CanHandleAuthentification(httpContext) && httpContext.Request.Headers[PrincipalIdpHeaderName] == "facebook" && IsHeaderSet(httpContext.Request.Headers, AuthTokenHeaderNames.FacebookAccessToken);
    }
}
