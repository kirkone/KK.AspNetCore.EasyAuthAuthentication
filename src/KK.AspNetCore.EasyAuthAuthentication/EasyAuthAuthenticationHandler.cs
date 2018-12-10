namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq; // required by Children<JObject>.FirstOrDefault requires using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
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

            if ((this.Context.User == null || this.Context.User.Identity == null || this.Context.User.Identity.IsAuthenticated == false)
                && !string.IsNullOrEmpty(this.Context.Request.Headers["X-MS-TOKEN-AAD-ID-TOKEN"].ToString()))
            {
                // build up identity from X-MS-TOKEN-AAD-ID-TOKEN header set by EasyAuth filters if user openid connect session cookie or oauth bearer token authenticated ...
                var ticket = this.BuildIdentityFromEasyAuthRequestHeaders(this.Context.Request.Headers);

                this.Logger.LogInformation("Set identity to user context object.");
                this.Context.User = ticket.Principal;

                this.Logger.LogInformation("identity build was a success, returning ticket");
                return AuthenticateResult.Success(ticket);
            }
            else if ((this.Context.User == null || this.Context.User.Identity == null || this.Context.User.Identity.IsAuthenticated == false)
                && string.IsNullOrEmpty(this.Context.Request.Headers["X-MS-TOKEN-AAD-ID-TOKEN"].ToString())
                && (this.Context.Request.Host.Value.StartsWith("localhost") && this.Context.Request.Path != "/" + $"{this.Options.AuthEndpoint}"))
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
                var ticket = this.BuildIdentityFromEasyAuthMeJson((JObject)payload[0]);

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

        private AuthenticationTicket BuildIdentityFromEasyAuthRequestHeaders(Microsoft.AspNetCore.Http.IHeaderDictionary requestHeaders)
        {
            var name = requestHeaders["X-MS-CLIENT-PRINCIPAL-NAME"][0];
            this.Logger.LogDebug("payload was fetched from easyauth headers, name: {0}", name);

            //var identity = new GenericIdentity(name, "AuthenticationTypes.Federation"); // setting ClaimsIdentity.AuthenticationType to value that azuread non-easyauth setups use

            this.Logger.LogInformation("building claims from payload...");

            var xMsClientPrincipal = JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(requestHeaders["X-MS-CLIENT-PRINCIPAL"][0])));
            //var nameidentifier = xMsClientPrincipal["claims"].Children<JObject>().FirstOrDefault(c => c["typ"].ToString() == ClaimTypes.NameIdentifier)?["val"].ToString();
            var claims = new List<Claim>();
            foreach (var claim in xMsClientPrincipal["claims"].Children<JObject>())
            {
                if (claim["typ"].ToString() == "http://schemas.microsoft.com/claims/authnmethodsreferences")
                {
                    foreach (var item in claim["val"].ToString().Split(','))
                    {
                        claims.Add(new Claim(ClaimTypes.Authentication, item));
                    }
                }
                else if (claim["typ"].ToString() == "roles")
                {
                    foreach (var item in claim["val"].ToString().Split(','))
                    {
                        //(User.Identity as ClaimsIdentity).RoleClaimType must match type that role claims are assigned to for Authorization and IsInRole to work
                        claims.Add(new Claim(ClaimTypes.Role, item));
                    }
                }
                else
                {
                    //(User.Identity as ClaimsIdentity).NameClaimType must be what name claim is assigned to for User.Identity.Name to work
                    claims.Add(new Claim(claim["typ"].ToString(), claim["val"].ToString()));
                }
            }

            this.Logger.LogInformation("Add claims to new identity");

            //identity.AddClaims(claims);
            var identity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation"); // setting ClaimsIdentity.AuthenticationType to value that azuread non-easyauth setups use

            //identity.AddClaim(new Claim("id_token", idToken)); // don't think we should be including this
            //identity.AddClaim(new Claim("http://schemas.microsoft.com/claims/authnclassreference", 1)); // don't think we need to add this
            if (!(identity.Claims as List<Claim>).Exists(claim => claim.Type == "scp")) identity.AddClaim(new Claim("scp", "user_impersonation")); // not sure why easyauth is dropping this
            identity.AddClaim(new Claim("provider_name", requestHeaders["X-MS-CLIENT-PRINCIPAL-IDP"][0]));
            //var principal = new GenericPrincipal(identity, null); // maybe passing valid string[] roles for second parameter would make IsInRole work but no need just use ClaimsPrincipal
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(principal, EasyAuthAuthenticationDefaults.AuthenticationScheme);
        }

        private AuthenticationTicket BuildIdentityFromEasyAuthMeJson(JObject payload)
        {
            var name = payload["user_id"].Value<string>(); // X-MS-CLIENT-PRINCIPAL-NAME
            this.Logger.LogDebug("payload was fetched from easyauth me json, name: {0}", name);

            //var identity = new GenericIdentity(name, "AuthenticationTypes.Federation"); // setting ClaimsIdentity.AuthenticationType to value that azuread non-easyauth setups use

            this.Logger.LogInformation("building claims from payload...");

            var claims = new List<Claim>();
            foreach (var claim in payload["user_claims"])
            {
                if (claim["typ"].ToString() == "http://schemas.microsoft.com/claims/authnmethodsreferences")
                {
                    foreach (var item in claim["val"].ToString().Split(','))
                    {
                        claims.Add(new Claim(ClaimTypes.Authentication, item));
                    }
                }
                else if (claim["typ"].ToString() == "roles")
                {
                    foreach (var item in claim["val"].ToString().Split(','))
                    {
                        //(User.Identity as ClaimsIdentity).RoleClaimType must match type that role claims are assigned to for Authorization and IsInRole to work
                        claims.Add(new Claim(ClaimTypes.Role, item));
                    }
                }
                else
                {
                    //(User.Identity as ClaimsIdentity).NameClaimType must be what name claim is assigned to for User.Identity.Name to work
                    claims.Add(new Claim(claim["typ"].ToString(), claim["val"].ToString()));
                }
            }

            this.Logger.LogInformation("Add claims to new identity");

            //identity.AddClaims(claims);
            var identity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation"); // setting ClaimsIdentity.AuthenticationType to value that azuread non-easyauth setups use

            //identity.AddClaim(new Claim("id_token", idToken)); // don't think we should be including this
            //identity.AddClaim(new Claim("http://schemas.microsoft.com/claims/authnclassreference", 1)); // don't think we need to add this
            if (!(identity.Claims as List<Claim>).Exists(claim => claim.Type == "scp")) identity.AddClaim(new Claim("scp", "user_impersonation")); // not sure why easyauth is dropping this
            identity.AddClaim(new Claim("provider_name", payload["provider_name"].Value<string>())); // X-MS-CLIENT-PRINCIPAL-IDP
            //var principal = new GenericPrincipal(identity, null); // maybe passing valid string[] roles for second parameter would make IsInRole work but no need just use ClaimsPrincipal
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(principal, EasyAuthAuthenticationDefaults.AuthenticationScheme);
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
            var authMeEndpoint = string.Empty;
            if (this.Options.AuthEndpoint.StartsWith("http")) authMeEndpoint = this.Options.AuthEndpoint; // enable pulling from places like storage account public blob container
            else authMeEndpoint = $"{uriString}/{this.Options.AuthEndpoint}"; // localhost relative path, e.g. wwwroot/.auth/me.json
            
            var request = new HttpRequestMessage(HttpMethod.Get, authMeEndpoint);
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
