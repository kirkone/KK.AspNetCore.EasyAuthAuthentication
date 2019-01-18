namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
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

        private static readonly Func<IHeaderDictionary, ClaimsPrincipal, HttpRequest, string, bool> CanUseEasyAuthJson =
            (headers, user, request, authEndpoint) =>
                IsContextUserNotAuthenticated(user)
                && !IsHeaderSet(headers, AuthTokenHeaderNames.AADIdToken)
                && request.Path != "/" + $"{authEndpoint}";

        private readonly Func<IHeaderDictionary, ClaimsPrincipal, bool> canUseHeaderAuth =
            (headers, user) => IsContextUserNotAuthenticated(user) &&
            IsHeaderSet(headers, AuthTokenHeaderNames.AADIdToken);

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

        /// <inheritdoc/>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            this.Logger.LogInformation("starting authentication handler for app service authentication");

            if (this.canUseHeaderAuth(this.Context.Request.Headers, this.Context.User))
            {
                return EasyAuthWithHeaderService.AuthUser(this.Logger, this.Context);
            }
            else if (CanUseEasyAuthJson(this.Context.Request.Headers, this.Context.User, this.Context.Request, this.Options.AuthEndpoint))
            {
                return await EasyAuthWithAuthMeService.AuthUser(this.Logger, this.Context, this.Options.AuthEndpoint);
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
