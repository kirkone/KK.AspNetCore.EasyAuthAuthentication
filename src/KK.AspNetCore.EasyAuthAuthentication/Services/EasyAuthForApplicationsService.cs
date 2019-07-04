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

    internal class EasyAuthForApplicationsService : IEasyAuthAuthentificationService
    {
        private const string AuthorizationHeader = "Authorization";
        private const string JWTIdentifyer = "Bearer";
        private const string ProviderNameKey = "idp";

        private static readonly Func<IHeaderDictionary, string, bool> IsHeaderSet =
           (headers, headerName) => !string.IsNullOrEmpty(headers[headerName].ToString());

        private readonly EasyAuthAuthenticationOptions defaultOptions = new EasyAuthAuthenticationOptions()
        {
            NameClaimType = ClaimTypes.Spn
        };

        private readonly ILogger<EasyAuthForApplicationsService> logger;

        public EasyAuthForApplicationsService(ILogger<EasyAuthForApplicationsService> logger) => this.logger = logger;

        public AuthenticateResult AuthUser(HttpContext context, EasyAuthAuthenticationOptions options = null)
        {
            if (options == null)
            {
                options = this.defaultOptions;
            }

            var tokenJson = this.GetTokenJson(context.Request.Headers[AuthorizationHeader].FirstOrDefault());
            var claims = this.BuildFromApplicationAuth(tokenJson, options);
            var ticket = AuthenticationTicketBuilder.Build(claims, tokenJson[ProviderNameKey].ToString(), options);
            return AuthenticateResult.Success(ticket);
        }

        public bool CanHandleAuthentification(HttpContext httpContext) => IsHeaderSet(httpContext.Request.Headers, AuthorizationHeader) &&
                httpContext.Request.Headers[AuthorizationHeader].FirstOrDefault().Contains(JWTIdentifyer);

        private IEnumerable<ClaimsModel> BuildFromApplicationAuth(JObject xMsClientPrincipal, EasyAuthAuthenticationOptions options)
        {
            this.logger.LogDebug($"payload was {xMsClientPrincipal["roles"].ToString()}");
            var claims = JsonConvert.DeserializeObject<IEnumerable<string>>(xMsClientPrincipal["roles"].ToString())
                    .Select(r => new ClaimsModel() { Typ = "roles", Values = r })
                    .ToList();
            claims.Add(new ClaimsModel() { Typ = options.NameClaimType, Values = xMsClientPrincipal["appid"].ToString() });
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
