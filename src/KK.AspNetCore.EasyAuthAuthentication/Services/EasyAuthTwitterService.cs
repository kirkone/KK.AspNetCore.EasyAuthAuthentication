namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System.Security.Claims;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class EasyAuthTwitterService : EasyAuthWithHeaderService<EasyAuthTwitterService>, IEasyAuthAuthentificationService
    {
        private readonly ILogger<EasyAuthTwitterService> logger;

        public EasyAuthTwitterService(ILogger<EasyAuthTwitterService> logger) : base(logger)
        {
            this.logger = logger;
            this.defaultOptions = new ProviderOptions(typeof(EasyAuthTwitterService).Name, "name", ClaimTypes.Role);
        }

        public new AuthenticateResult AuthUser(HttpContext context)
        {
            this.logger.LogInformation("Try authentification with twitter account.");
            return base.AuthUser(context);
        }

        public new AuthenticateResult AuthUser(HttpContext context, ProviderOptions options)
        {
            this.logger.LogInformation("Try authentification with twitter account.");
            return base.AuthUser(context, options);
        }

        public new bool CanHandleAuthentification(HttpContext httpContext) => base.CanHandleAuthentification(httpContext) && httpContext.Request.Headers[PrincipalIdpHeaderName] == "twitter" && IsHeaderSet(httpContext.Request.Headers, AuthTokenHeaderNames.TwitterAccessToken);
    }
}
