namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class EasyAuthAuthenticationHandler : AuthenticationHandler<EasyAuthAuthenticationOptions>
    {
        public EasyAuthAuthenticationHandler(
            IOptionsMonitor<EasyAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
        ) : base(options, logger, encoder, clock)
        {

        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Logger.LogInformation("starting authentication handler for app service authentication");

            if (
                this.Context.User == null ||
                this.Context.User.Identity == null ||
                this.Context.User.Identity.IsAuthenticated == false
            )
            {
                var cookieContainer = new CookieContainer();
                HttpClientHandler handler = createHandler(ref cookieContainer);
                HttpRequestMessage httpRequest = CreateAuthRequest(ref cookieContainer);

                JArray payload = null;
                try
                {
                    payload = await getAuthMe(handler, httpRequest);
                }
                catch (Exception ex)
                {
                    return AuthenticateResult.Fail(ex.Message);
                }

                //build up identity from json...
                AuthenticationTicket ticket = BuildIdentityFromJsonPayload((JObject)payload[0]);

                Logger.LogInformation("Set identity to user context object.");
                this.Context.User = ticket.Principal;

                Logger.LogInformation("identity build was a success, returning ticket");
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                Logger.LogInformation("identity already set, skipping middleware");
                return AuthenticateResult.NoResult();
            }
        }

        private AuthenticationTicket BuildIdentityFromJsonPayload(JObject payload)
        {
            var id = payload["user_id"].Value<string>();
            var idToken = payload["id_token"].Value<string>();
            var providerName = payload["provider_name"].Value<string>();

            Logger.LogDebug("payload was fetched from endpoint. id: {0}", id);

            var identity = new GenericIdentity(id);

            Logger.LogInformation("building claims from payload...");

            List<Claim> claims = new List<Claim>();
            foreach (var claim in payload["user_claims"])
            {
                claims.Add(new Claim(claim["typ"].ToString(), claim["val"].ToString()));
            }

            Logger.LogInformation("Add claims to new identity");

            identity.AddClaims(claims);
            identity.AddClaim(new Claim("id_token", idToken));
            identity.AddClaim(new Claim("provider_name", providerName));
            var p = new GenericPrincipal(identity, null);
            return new AuthenticationTicket(
                p,
                EasyAuthAuthenticationDefaults.AuthenticationScheme
            );
        }

        private HttpRequestMessage CreateAuthRequest(ref CookieContainer cookieContainer)
        {
            Logger.LogInformation($"identity not found, attempting to fetch from auth endpoint '/{Options.AuthEndpoint}'");

            var uriString = $"{Context.Request.Scheme}://{Context.Request.Host}";

            Logger.LogDebug("host uri: {0}", uriString);

            foreach (var c in Context.Request.Cookies)
            {
                cookieContainer.Add(new Uri(uriString), new Cookie(c.Key, c.Value));
            }

            Logger.LogDebug("found {0} cookies in request", cookieContainer.Count);

            foreach (var cookie in Context.Request.Cookies)
            {
                Logger.LogDebug(cookie.Key);
            }

            //fetch value from endpoint
            var request = new HttpRequestMessage(HttpMethod.Get, $"{uriString}/{Options.AuthEndpoint}");
            foreach (var header in Context.Request.Headers)
            {
                if (header.Key.StartsWith("X-ZUMO-"))
                {
                    request.Headers.Add(header.Key, header.Value[0]);
                }
            }
            return request;
        }

        private static HttpClientHandler createHandler(ref CookieContainer container)
        {
            var handler = new HttpClientHandler()
            {
                CookieContainer = container
            };
            return handler;
        }

        private async Task<JArray> getAuthMe(HttpClientHandler handler, HttpRequestMessage httpRequest)
        {
            JArray payload = null;
            using (HttpClient client = new HttpClient(handler))
            {
                var response = await client.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogDebug("auth endpoint was not sucessful. Status code: {0}, reason {1}", response.StatusCode, response.ReasonPhrase);
                    throw new WebException("Unable to fetch user information from auth endpoint.");
                }

                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    payload = JArray.Parse(content);
                }
                catch (Exception)
                {
                    throw new JsonSerializationException("Could not retreive json from /me endpoint.");
                }

            };
            return payload;
        }
    }
}