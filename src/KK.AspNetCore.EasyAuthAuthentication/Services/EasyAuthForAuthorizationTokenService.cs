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
        private const string IdentityProviderKey = "idp";
        private const string IssuerKey = "iss";

        private static readonly Func<IHeaderDictionary, string, bool> IsHeaderSet =
           (headers, headerName) => !string.IsNullOrEmpty(headers[headerName].ToString());

        private readonly ProviderOptions defaultOptions = new ProviderOptions(typeof(EasyAuthForAuthorizationTokenService).Name, ClaimTypes.Spn, "roles");

        private readonly ILogger<EasyAuthForAuthorizationTokenService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyAuthForAuthorizationTokenService"/> class.
        /// </summary>
        /// <param name="logger">The logger for this service.</param>
        public EasyAuthForAuthorizationTokenService(ILogger<EasyAuthForAuthorizationTokenService> logger) => this.logger = logger;

        /// <inheritdoc/>
        public AuthenticateResult AuthUser(HttpContext context) => this.AuthUser(context, null);

        /// <inheritdoc/>
        public AuthenticateResult AuthUser(HttpContext context, ProviderOptions? options)
        {
            this.defaultOptions.ChangeModel(options);

            var tokenJson = this.GetTokenJson(context.Request.Headers[AuthorizationHeader].FirstOrDefault());
            var claims = this.BuildFromAuthToken(tokenJson, this.defaultOptions);
            var identityProviderClaim = tokenJson[IdentityProviderKey]?.ToString();
            if (identityProviderClaim == null)
            {
                /* As stated here (https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens), we
                * can use issuer-claim as identity provider, if idp-claim isn't defined in the token
                */
                identityProviderClaim = tokenJson[IssuerKey]?.ToString();
            }
            if (string.IsNullOrWhiteSpace(identityProviderClaim?.ToString()))
            {
                throw new ArgumentException($"In the AAD authentification token are {IdentityProviderKey} and {IssuerKey} missing. This isn't a valid token.");
            }
            var ticket = AuthenticationTicketBuilder.Build(claims, identityProviderClaim, this.defaultOptions);
            return AuthenticateResult.Success(ticket);
        }

        /// <inheritdoc/>
        public bool CanHandleAuthentification(HttpContext httpContext) =>
            IsHeaderSet(httpContext.Request.Headers, AuthorizationHeader) &&
            httpContext.Request.Headers[AuthorizationHeader].FirstOrDefault().Contains(JWTIdentifier);

        private IEnumerable<AADClaimsModel> BuildFromAuthToken(JObject xMsClientPrincipal, ProviderOptions options)
        {
            var claims = new List<AADClaimsModel>();

            if (xMsClientPrincipal.ContainsKey(this.defaultOptions.RoleClaimType))
            {
                this.logger.LogDebug($"payload was {xMsClientPrincipal[this.defaultOptions.RoleClaimType]}");

                claims.AddRange(JsonConvert.DeserializeObject<IEnumerable<string>>(xMsClientPrincipal[this.defaultOptions.RoleClaimType].ToString())
                        .Select(r => new AADClaimsModel { Typ = this.defaultOptions.RoleClaimType, Values = r }));
            }
            var otherClaims = xMsClientPrincipal.Properties()
                .Where(claimToken => claimToken.Name != this.defaultOptions.RoleClaimType)
                .Select(claimToken => new AADClaimsModel { Typ = claimToken.Name, Values = claimToken.Value.ToString() })
                .ToList();
            claims.AddRange(otherClaims);

            string nameClaimValue;
            if (xMsClientPrincipal.ContainsKey("upn")) // AAD "upn" user auth claim
            {
                nameClaimValue = xMsClientPrincipal["upn"].ToString();
            }
            else if (xMsClientPrincipal.ContainsKey("appid")) // AAD "appid" application auth claim
            {
                nameClaimValue = xMsClientPrincipal["appid"].ToString();
            }
            else if (xMsClientPrincipal.ContainsKey("sub")) // JWT standard "sub"ject claim
            {
                nameClaimValue = xMsClientPrincipal["sub"].ToString();
            }
            else
            {
                throw new ArgumentException("Provided JWT token is missing a subject, user ID, or app ID claim", nameof(xMsClientPrincipal));
            }
            claims.Add(new AADClaimsModel
            {
                Typ = options.NameClaimType,
                Values = nameClaimValue
            });

            return claims;
        }

        private JObject GetTokenJson(string headerContent)
        {
            var cleanupToken = headerContent
                            .Replace(JWTIdentifier, string.Empty)
                            .Replace(" ", string.Empty)
                            .Split('.')[1];
            var cleanupTokenBuilder = new StringBuilder(cleanupToken);
            while (cleanupTokenBuilder.Length % 4 != 0)
            {
                cleanupTokenBuilder.Append("=");
            }
            cleanupToken = cleanupTokenBuilder.ToString();

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
