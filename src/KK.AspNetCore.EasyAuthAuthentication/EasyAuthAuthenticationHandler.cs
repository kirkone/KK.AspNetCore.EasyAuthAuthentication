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

        private static readonly Func<IHeaderDictionary, ClaimsPrincipal, HttpRequest, EasyAuthAuthenticationOptions, bool> CanUseEasyAuthJson =
            (headers, user, request, options) =>
                IsContextUserNotAuthenticated(user)
                && !IsHeaderSet(headers, AuthTokenHeaderNames.AADIdToken)
                && request.Path != "/" + $"{options.AuthEndpoint}";

        private readonly IEnumerable<IEasyAuthAuthentificationService> authenticationServices;

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
            ISystemClock clock) : base(options, logger, encoder, clock) => this.authenticationServices = authenticationServices;

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
                return await EasyAuthWithAuthMeService.AuthUser(this.Logger, this.Context, this.Options);
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
