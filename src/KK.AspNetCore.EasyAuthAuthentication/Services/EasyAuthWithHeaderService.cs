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
        private const string PrincipalNameHeader = "X-MS-CLIENT-PRINCIPAL-NAME";
        /// <summary>
        /// JWT
        /// </summary>
        private const string PrincipalObjectHeader = "X-MS-CLIENT-PRINCIPAL";
        private const string PrincipalIdpHeaderName = "X-MS-CLIENT-PRINCIPAL-IDP";
        private ILogger Logger { get; }
        private IHeaderDictionary Headers { get; }

        private EasyAuthWithHeaderService(
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
        /// <returns>An <see cref="AuthenticationTicket" /></returns>
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
            var name = this.Headers[PrincipalNameHeader][0];
            this.Logger.LogDebug($"payload was fetched from EasyAuth headers, name: {name}");

            this.Logger.LogInformation("building claims from payload...");
            var xMsClientPrincipal = JObject.Parse(
                                        Encoding.UTF8.GetString(
                                            Convert.FromBase64String(this.Headers[PrincipalObjectHeader][0])
                                        )
                                    );

            var claims = xMsClientPrincipal["claims"].Children<JObject>();
            var providerName = this.Headers[PrincipalIdpHeaderName][0];

            return AuthenticationTicketBuilder.Build(claims, providerName);
        }
    }
}