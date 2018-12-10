namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq; // required by Children<JObject>.FirstOrDefault requires using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using KK.AspNetCore.EasyAuthAuthentication.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Enables the handler in an Easy Auth context.
    /// </summary>
    public class EasyAuthAuthenticationHandler : AuthenticationHandler<EasyAuthAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EasyAuthAuthenticationHandler"/> class.
        /// </summary>
        /// <param name="options">An instance of <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <param name="logger">An instance of <see cref="ILoggerFactory"/>.</param>
        /// <param name="encoder">An instance of <see cref="UrlEncoder"/>.</param>
        /// <param name="clock">An instance of <see cref="ISystemClock"/>.</param>
        public EasyAuthAuthenticationHandler(
            IOptionsMonitor<EasyAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        private static Func<ClaimsPrincipal, bool> isContextUserNotAuthenticated =
            user => (user == null || user.Identity == null || user.Identity.IsAuthenticated == false);
        private static Func<IHeaderDictionary, string, bool> isHeaderSet =
            (headers, headerName) => !string.IsNullOrEmpty(headers[headerName].ToString());
        private Func<IHeaderDictionary, ClaimsPrincipal, bool> canUseHeaderAuth =
            (headers, user) => isContextUserNotAuthenticated(user) &&
            isHeaderSet(headers, AuthTokenHeaderNames.AADIdToken);
        private static Func<IHeaderDictionary, ClaimsPrincipal, HttpRequest, string, bool> canUseEasyAuthJson =
            (headers, user, request, authEndpoint) =>
                isContextUserNotAuthenticated(user)
                && !isHeaderSet(headers, AuthTokenHeaderNames.AADIdToken)
                && request.Path != "/" + $"{authEndpoint}";

        /// <inheritdoc/>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            this.Logger.LogInformation("starting authentication handler for app service authentication");

            if (canUseHeaderAuth(this.Context.Request.Headers, this.Context.User))
            {
                return EasyAuthWithHeaderService.AuthUser(this.Logger, this.Context);
            }
            else if (canUseEasyAuthJson(this.Context.Request.Headers, this.Context.User, this.Context.Request, this.Options.AuthEndpoint))
            {
                return await EasyAuthWithAuthMeService.AuthUser(this.Logger, this.Context, this.Options.AuthEndpoint);
            }
            else
            {
                if (isContextUserNotAuthenticated(this.Context.User))
                {
                    this.Logger.LogInformation("The identity isn't set by easy auth.");
                }
                else
                {
                    this.Logger.LogInformation("identity already set, skipping middleware");
                }
                return AuthenticateResult.NoResult();
            }
        }
    }
}