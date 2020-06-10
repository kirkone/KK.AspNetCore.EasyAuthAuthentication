namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Enables the handler in an Easy Auth context.
    /// </summary>
    public class EasyAuthAuthenticationHandler : AuthenticationHandler<EasyAuthAuthenticationOptions>
    {
        private static readonly Func<ClaimsPrincipal, bool> IsContextUserNotAuthenticated =
            user => user == null || user.Identity == null || user.Identity.IsAuthenticated == false;

        private static readonly Func<IHeaderDictionary, string, bool> IsHeaderSet =
            (headers, headerName) => !string.IsNullOrEmpty(headers[headerName].ToString());

        private static Func<IHeaderDictionary, ClaimsPrincipal, HttpRequest, EasyAuthAuthenticationOptions, bool> CanUseEasyAuthJson =
            (headers, user, request, options) =>
                IsContextUserNotAuthenticated(user)
                && !IsHeaderSet(headers, AuthTokenHeaderNames.AADIdToken);

        private readonly IEnumerable<IEasyAuthAuthentificationService> authenticationServices;
        private readonly IConfiguration appConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyAuthAuthenticationHandler"/> class.
        /// </summary>
        /// <param name="options">An instance of <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <param name="authenticationServices">All implementation os <see cref="IEasyAuthAuthentificationService"/> that are in the DI.</param>
        /// <param name="logger">An instance of <see cref="ILoggerFactory"/>.</param>
        /// <param name="encoder">An instance of <see cref="UrlEncoder"/>.</param>
        /// <param name="clock">An instance of <see cref="ISystemClock"/>.</param>
        public EasyAuthAuthenticationHandler(
            IOptionsMonitor<EasyAuthAuthenticationOptions> options,
            IEnumerable<IEasyAuthAuthentificationService> authenticationServices,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration appConfiguration) : base(options, logger, encoder, clock)
        {
            this.authenticationServices = authenticationServices;
            this.appConfiguration = appConfiguration;
            this.SetupHandler();
        }

        private void SetupHandler()
        {
            var authEnabled = this.appConfiguration.GetValue<bool?>("APPSETTING_WEBSITE_AUTH_ENABLED");
            var allowAnoymos = this.appConfiguration.GetValue<string?>("APPSETTING_WEBSITE_AUTH_UNAUTHENTICATED_ACTION") == "AllowAnonymous" ? true : false;
            if(authEnabled == null || authEnabled == false)
            {
                // auth is turned of. So hopefully the user is in local debugging.
                return;
            }
            if (allowAnoymos == true)
            {
                this.Logger.LogError("Don't allow anonymous requests! The easy auth extension don't check the token!");
                throw new ArgumentException("Don't allow anonymous requests");
            }
            // disable the local auth.me json in azure.
            CanUseEasyAuthJson = (h, u, r, o) => false;
        }

        /// <inheritdoc/>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            this.Logger.LogInformation("starting authentication handler for app service authentication");

            var authService = this.authenticationServices.FirstOrDefault(d => d.CanHandleAuthentification(this.Context));
            var enabledProviders = this.Options.ProviderOptions.Where(d => d.Enabled == true);
            if (authService != null && enabledProviders.Any(d => d.ProviderName == authService.GetType().Name))
            {
                this.Logger.LogInformation($"use the {authService.GetType().Name} as auth handler.");
                return authService.AuthUser(this.Context, this.Options.ProviderOptions.FirstOrDefault(d => d.ProviderName == authService.GetType().Name));
            }
            else if (CanUseEasyAuthJson(this.Context.Request.Headers, this.Context.User, this.Context.Request, this.Options))
            {
                var service = new LocalAuthMeService(this.Logger,
                    this.Context.Request.Scheme,
                    this.Context.Request.Host.ToString(),
                    this.Context.Request.Cookies,
                    this.Context.Request.Headers);
                return await service.AuthUser(this.Context, this.Options.LocalProviderOption);
            }
            else
            {
                if (IsContextUserNotAuthenticated(this.Context.User))
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
