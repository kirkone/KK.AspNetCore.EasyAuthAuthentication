namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using KK.AspNetCore.EasyAuthAuthentication.Interfaces;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A service that can be used to authenticate applications or users in the <see cref="EasyAuthAuthenticationHandler"/>.
    /// That is for the use case if the user doesn't authenticated on this webapp. So he has only a Authorization Header.
    /// </summary>
    public class EasyAuthForAuthorizationTokenService : IEasyAuthAuthentificationService
    {
        private const string AuthorizationHeader = "Authorization";
        private const string JWTIdentifier = "Bearer";
        private const string ProviderNameKey = "idp";

        private static readonly Func<IHeaderDictionary, string, bool> IsHeaderSet =
           (headers, headerName) => !string.IsNullOrEmpty(headers[headerName].ToString());

        private readonly ProviderOptions defaultOptions = new ProviderOptions(typeof(EasyAuthForAuthorizationTokenService).Name)
        {
            NameClaimType = ClaimTypes.Spn,
            RoleClaimType = "roles"
        };

        private readonly ILogger<EasyAuthForAuthorizationTokenService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyAuthForAuthorizationTokenService"/> class.
        /// </summary>
        /// <param name="logger">The logger for this service.</param>
        public EasyAuthForAuthorizationTokenService(ILogger<EasyAuthForAuthorizationTokenService> logger) => this.logger = logger;

        /// <inheritdoc/>
        public AuthenticateResult AuthUser(HttpContext context, ProviderOptions options = null)
        {
            this.defaultOptions.ChangeModel(options);

            var tokenJson = this.GetTokenJson(context.Request.Headers[AuthorizationHeader].FirstOrDefault());
            var claims = this.BuildFromApplicationAuth(tokenJson, this.defaultOptions);
            var ticket = AuthenticationTicketBuilder.Build(claims, tokenJson[ProviderNameKey].ToString(), this.defaultOptions);
            return AuthenticateResult.Success(ticket);
        }

        /// <inheritdoc/>
        public bool CanHandleAuthentification(HttpContext httpContext) =>
            IsHeaderSet(httpContext.Request.Headers, AuthorizationHeader) &&
            httpContext.Request.Headers[AuthorizationHeader].FirstOrDefault().Contains(JWTIdentifier);

        private IEnumerable<AADClaimsModel> BuildFromApplicationAuth(JObject xMsClientPrincipal, ProviderOptions options)
        {
            this.logger.LogDebug($"payload was {xMsClientPrincipal[this.defaultOptions.RoleClaimType].ToString()}");

            var claims = JsonConvert.DeserializeObject<IEnumerable<string>>(xMsClientPrincipal[this.defaultOptions.RoleClaimType].ToString())
                    .Select(r => new AADClaimsModel() { Typ = this.defaultOptions.RoleClaimType, Values = r })
                    .ToList();
            var otherClaims = xMsClientPrincipal.Properties()
                .Where(claimToken => claimToken.Name != this.defaultOptions.RoleClaimType)
                .Select(claimToken => new AADClaimsModel() { Typ = claimToken.Name, Values = claimToken.Value.ToString() })
                .ToList();
            claims.AddRange(otherClaims);
            claims.Add(new AADClaimsModel() { Typ = options.NameClaimType, Values = xMsClientPrincipal["appid"].ToString() });
            return claims;
        }

        private JObject GetTokenJson(string headerContent)
        {
            var cleanupToken = headerContent
                            .Replace(JWTIdentifier, string.Empty)
                            .Replace(" ", string.Empty)
                            .Split('.')[1];
            while (cleanupToken.Length % 4 != 0)
            {
                cleanupToken += "=";
            }

            this.logger.LogDebug($"Cleanup token is: {cleanupToken}");
            var xMsClientPrincipal = JObject.Parse(
                       Encoding.UTF8.GetString(
                           Convert.FromBase64String(cleanupToken)
                       )
                   );
            return xMsClientPrincipal;
        }
    }
}
