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
    /// A service that can be used to authentificat applications in the <see cref="EasyAuthAuthenticationHandler"/>.
    /// </summary>
    public class EasyAuthForApplicationsService : IEasyAuthAuthentificationService
    {
        private const string AuthorizationHeader = "Authorization";
        private const string JWTIdentifyer = "Bearer";
        private const string ProviderNameKey = "idp";

        private static readonly Func<IHeaderDictionary, string, bool> IsHeaderSet =
           (headers, headerName) => !string.IsNullOrEmpty(headers[headerName].ToString());

        private readonly ProviderOptions defaultOptions = new ProviderOptions(typeof(EasyAuthForApplicationsService).Name)
        {
            NameClaimType = ClaimTypes.Spn
        };

        private readonly ILogger<EasyAuthForApplicationsService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyAuthForApplicationsService"/> class.
        /// </summary>
        /// <param name="logger">The logger for this service.</param>
        public EasyAuthForApplicationsService(ILogger<EasyAuthForApplicationsService> logger) => this.logger = logger;

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
            httpContext.Request.Headers[AuthorizationHeader].FirstOrDefault().Contains(JWTIdentifyer);

        private IEnumerable<AADClaimsModel> BuildFromApplicationAuth(JObject xMsClientPrincipal, ProviderOptions options)
        {
            this.logger.LogDebug($"payload was {xMsClientPrincipal["roles"].ToString()}");
            var claims = JsonConvert.DeserializeObject<IEnumerable<string>>(xMsClientPrincipal["roles"].ToString())
                    .Select(r => new AADClaimsModel() { Typ = "roles", Values = r })
                    .ToList();
            claims.Add(new AADClaimsModel() { Typ = options.NameClaimType, Values = xMsClientPrincipal["appid"].ToString() });
            return claims;
        }

        private JObject GetTokenJson(string headerContent)
        {
            var cleanupToken = headerContent
                            .Replace("Bearer", string.Empty)
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
