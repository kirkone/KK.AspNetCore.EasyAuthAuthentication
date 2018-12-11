namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System;
    using System.Text;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    internal class EasyAuthWithHeaderService
    {
        private const string PrincipalNameHeader = "X-MS-CLIENT-PRINCIPAL-NAME";
        private const string PrincipalObjectHeader = "X-MS-CLIENT-PRINCIPAL";
        private const string PrincipalIdpHeaderName = "X-MS-CLIENT-PRINCIPAL-IDP";

        private EasyAuthWithHeaderService(
            ILogger logger,
            IHeaderDictionary headers)
        {
            this.Logger = logger;
            this.Headers = headers;
        }

        private ILogger Logger { get; }

        private IHeaderDictionary Headers { get; }

        /// <summary>
        /// build up identity from X-MS-TOKEN-AAD-ID-TOKEN header set by EasyAuth filters if user openId connect session cookie or oauth bearer token authenticated ...
        /// </summary>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        /// <param name="context">Http context of the request.</param>
        /// <returns>An <see cref="AuthenticateResult" />.</returns>
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