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

    /// <summary>
    /// Enables the handler in an Easy Auth context.
    /// </summary>
    public class EasyAuthAuthenticationHandler : AuthenticationHandler<EasyAuthAuthenticationOptions>
    {
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

            if (
                (this.Context.User == null ||
                this.Context.User.Identity == null ||
                this.Context.User.Identity.IsAuthenticated == false)
                && this.Context.Request.Path != "/" + $"{this.Options.AuthEndpoint}")
            {
                var cookieContainer = new CookieContainer();
                var handler = this.CreateHandler(ref cookieContainer);
                var httpRequest = this.CreateAuthRequest(ref cookieContainer);

                JArray payload = null;
                try
                {
                    payload = await this.GetAuthMe(handler, httpRequest);
                }
                catch (Exception ex)
                {
                    return AuthenticateResult.Fail(ex.Message);
                }

                // build up identity from json...
                var ticket = this.BuildIdentityFromJsonPayload((JObject)payload[0]);

                this.Logger.LogInformation("Set identity to user context object.");
                this.Context.User = ticket.Principal;

                this.Logger.LogInformation("identity build was a success, returning ticket");
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                this.Logger.LogInformation("identity already set, skipping middleware");
                return AuthenticateResult.NoResult();
            }
        }

        private AuthenticationTicket BuildIdentityFromJsonPayload(JObject payload)
        {
            var id = payload["user_id"].Value<string>();
            var idToken = payload["id_token"].Value<string>();
            var providerName = payload["provider_name"].Value<string>();

            this.Logger.LogDebug("payload was fetched from endpoint. id: {0}", id);

            var identity = new GenericIdentity(id);

            this.Logger.LogInformation("building claims from payload...");

            var claims = new List<Claim>();
            foreach (var claim in payload["user_claims"])
            {
                claims.Add(new Claim(claim["typ"].ToString(), claim["val"].ToString()));
            }

            this.Logger.LogInformation("Add claims to new identity");

            identity.AddClaims(claims);
            identity.AddClaim(new Claim("id_token", idToken));
            identity.AddClaim(new Claim("provider_name", providerName));
            var p = new GenericPrincipal(identity, null);
            return new AuthenticationTicket(
                p,
                EasyAuthAuthenticationDefaults.AuthenticationScheme);
        }

        private HttpRequestMessage CreateAuthRequest(ref CookieContainer cookieContainer)
        {
            this.Logger.LogInformation($"identity not found, attempting to fetch from auth endpoint '/{this.Options.AuthEndpoint}'");

            var uriString = $"{this.Context.Request.Scheme}://{this.Context.Request.Host}";

            this.Logger.LogDebug("host uri: {0}", uriString);

            foreach (var c in this.Context.Request.Cookies)
            {
                cookieContainer.Add(new Uri(uriString), new Cookie(c.Key, c.Value));
            }

            this.Logger.LogDebug("found {0} cookies in request", cookieContainer.Count);

            foreach (var cookie in this.Context.Request.Cookies)
            {
                this.Logger.LogDebug(cookie.Key);
            }

            // fetch value from endpoint
            var request = new HttpRequestMessage(HttpMethod.Get, $"{uriString}/{this.Options.AuthEndpoint}");
            foreach (var header in this.Context.Request.Headers)
            {
                if (header.Key.StartsWith("X-ZUMO-"))
                {
                    request.Headers.Add(header.Key, header.Value[0]);
                }
            }

            return request;
        }

        private HttpClientHandler CreateHandler(ref CookieContainer container)
        {
            var handler = new HttpClientHandler()
            {
                CookieContainer = container
            };
            return handler;
        }

        private async Task<JArray> GetAuthMe(HttpClientHandler handler, HttpRequestMessage httpRequest)
        {
            JArray payload = null;
            using (var client = new HttpClient(handler))
            {
                var response = await client.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    this.Logger.LogDebug("auth endpoint was not sucessful. Status code: {0}, reason {1}", response.StatusCode, response.ReasonPhrase);
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
            }

            return payload;
        }
    }
}
