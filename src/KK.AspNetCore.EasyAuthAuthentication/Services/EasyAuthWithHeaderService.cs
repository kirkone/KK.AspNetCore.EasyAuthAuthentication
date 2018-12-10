using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    public class EasyAuthWithHeaderService
    {
        public ILogger Logger { get; }
        public IHeaderDictionary Headers { get; }

        public EasyAuthWithHeaderService(
            ILogger logger,
            IHeaderDictionary headers
        )
        {
            Logger = logger;
            Headers = headers;
        }

        ///<summary>
        /// build up identity from X-MS-TOKEN-AAD-ID-TOKEN header set by EasyAuth filters if user openId connect session cookie or oauth bearer token authenticated ...
        /// </summary>
        /// <param name="logger">a logger</param>
        /// <param name="context">Http context of the request</param>
        /// <returns>An `AuthenticationTicket`</returns>
        public static AuthenticateResult AuthUser(ILogger logger, HttpContext context)
        {
            var service = new EasyAuthWithHeaderService(logger, context.Request.Headers);
            var ticket = service.BuildIdentityFromEasyAuthRequestHeaders();
            logger.LogInformation("Set identity to user context object.");
            context.User = ticket.Principal;
            logger.LogInformation("identity build was a success, returning ticket");
            return AuthenticateResult.Success(ticket);
        }

        private AuthenticationTicket BuildIdentityFromEasyAuthRequestHeaders()
        {
            var name = this.Headers["X-MS-CLIENT-PRINCIPAL-NAME"][0];
            this.Logger.LogDebug($"payload was fetched from easyauth headers, name: {name}");

            var identity = new GenericIdentity(name, AuthenticationTypesNames.Federation); // setting ClaimsIdentity.AuthenticationType to value that azureAd non-easyauth setups use

            this.Logger.LogInformation("building claims from payload...");

            var xMsClientPrincipal = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(this.Headers["X-MS-CLIENT-PRINCIPAL"][0])));
            var claims = xMsClientPrincipal["claims"].Children<JObject>();
            var providerName = this.Headers["X-MS-CLIENT-PRINCIPAL-IDP"][0];
            return AuthenticationTicketBuilder.Build(claims, name, providerName);
        }
    }
}