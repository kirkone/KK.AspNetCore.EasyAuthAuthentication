namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System.Security.Claims;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class EasyAuthMicrosoftService : EasyAuthWithHeaderService<EasyAuthMicrosoftService>, IEasyAuthAuthentificationService
    {
        private readonly ILogger<EasyAuthMicrosoftService> logger;

        public EasyAuthMicrosoftService(ILogger<EasyAuthMicrosoftService> logger) : base(logger)
        {
            this.logger = logger;
            this.defaultOptions = new ProviderOptions(typeof(EasyAuthMicrosoftService).Name, "name", ClaimTypes.Role);
        }

        public new AuthenticateResult AuthUser(HttpContext context)
        {
            this.logger.LogInformation("Try authentification with microsoft account.");
            return base.AuthUser(context);
        }

        public new AuthenticateResult AuthUser(HttpContext context, ProviderOptions options)
        {
            this.logger.LogInformation("Try authentification with microsoft account.");
            return base.AuthUser(context, options);
        }

        public new bool CanHandleAuthentification(HttpContext httpContext) => base.CanHandleAuthentification(httpContext) && httpContext.Request.Headers[PrincipalIdpHeaderName] == "microsoftaccount" && IsHeaderSet(httpContext.Request.Headers, AuthTokenHeaderNames.MicrosoftAccessToken);
    }
}
