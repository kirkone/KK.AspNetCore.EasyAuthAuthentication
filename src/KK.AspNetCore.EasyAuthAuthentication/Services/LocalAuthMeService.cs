namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class LocalAuthMeService
    {
        public LocalAuthMeService(
            ILogger logger,
            string httpSchema,
            string host,
            IRequestCookieCollection cookies,
            IHeaderDictionary headers)
        {
            this.HttpSchema = httpSchema;
            this.Host = host;
            this.Cookies = cookies;
            this.Headers = headers;
            this.Logger = logger;
        }

        private readonly LocalProviderOption defaultOptions = new LocalProviderOption(".auth/me.json", ClaimTypes.Name, ClaimTypes.Role);

        private string Host { get; }

        private IRequestCookieCollection Cookies { get; }

        private IHeaderDictionary Headers { get; }

        private ILogger Logger { get; }

        private string HttpSchema { get; }

        /// <summary>
        /// Use this method to authenticate a user with easy auth.
        /// This will set the `context.User` of your HttpContext.
        /// </summary>
        /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
        /// <param name="context">The http context with the missing user claim.</param>
        /// <param name="options">The <c>EasyAuthAuthenticationOptions</c> to use.</param>
        /// <returns>An <see cref="AuthenticateResult" />.</returns>
        public async Task<AuthenticateResult> AuthUser(HttpContext context, LocalProviderOption? options)
        {           
            this.defaultOptions.ChangeModel(options);
            if (context.Request.Path.ToString() == this.defaultOptions.AuthEndpoint || context.Request.Path.ToString() == $"/{this.defaultOptions.AuthEndpoint}")
            {
                return AuthenticateResult.Fail($"The path {this.defaultOptions.AuthEndpoint} doesn't exsists or don't contain a valid auth json.");
            }
            try
            {
                var ticket = await this.CreateUserTicket();
                this.Logger.LogInformation("Set identity to user context object.");
                context.User = ticket.Principal;
                this.Logger.LogInformation("identity build was a success, returning ticket");
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        private async Task<AuthenticationTicket> CreateUserTicket()
        {
            var cookieContainer = new CookieContainer();
            var handler = this.CreateHandler(ref cookieContainer);
            var httpRequest = this.CreateAuthRequest(ref cookieContainer);
            var payload = await this.GetAuthMe(handler, httpRequest);

            // build up identity from json...
            var ticket = this.BuildIdentityFromEasyAuthMeJson((JObject)payload[0]);
            this.Logger.LogInformation("Set identity to user context object.");
            return ticket;
        }

        private AuthenticationTicket BuildIdentityFromEasyAuthMeJson(JObject payload)
        {
            var providerName = payload["provider_name"].Value<string>();
            this.Logger.LogDebug($"payload was fetched from easyauth me json, provider: {providerName}");

            this.Logger.LogInformation("building claims from payload...");
            return AuthenticationTicketBuilder.Build(
                    JsonConvert.DeserializeObject<IEnumerable<AADClaimsModel>>(payload["user_claims"].ToString()),
                    providerName,
                    this.defaultOptions.GetProviderOptions()
                );
        }

        private async Task<JArray> GetAuthMe(HttpClientHandler handler, HttpRequestMessage httpRequest)
        {
            JArray? payload = null;
            using (var client = new HttpClient(handler))
            {
                HttpResponseMessage? response;
                try
                {
                    response = await client.SendAsync(httpRequest);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
                
                if (!response.IsSuccessStatusCode)
                {
                    this.Logger.LogDebug("auth endpoint was not successful. Status code: {0}, reason {1}", response.StatusCode, response.ReasonPhrase);
                    response.Dispose();
                    throw new WebException("Unable to fetch user information from auth endpoint.");
                }

                var content = await response.Content.ReadAsStringAsync();
                response.Dispose();
                try
                {
                    payload = JArray.Parse(content);
                }
                catch (Exception)
                {
                    throw new JsonSerializationException("Could not retrieve json from /me endpoint.");
                }
            }

            return payload;
        }

        private HttpRequestMessage CreateAuthRequest(ref CookieContainer cookieContainer)
        {
            this.Logger.LogInformation($"identity not found, attempting to fetch from auth endpoint '/{this.defaultOptions.AuthEndpoint}'");

            var uriString = $"{this.HttpSchema}://{this.Host}";

            this.Logger.LogDebug("host uri: {0}", uriString);

            foreach (var c in this.Cookies)
            {
                cookieContainer.Add(new Uri(uriString), new Cookie(c.Key, c.Value));
            }

            this.Logger.LogDebug("found {0} cookies in request", cookieContainer.Count);

            foreach (var cookie in this.Cookies)
            {
                this.Logger.LogDebug(cookie.Key);
            }

            // fetch value from endpoint
            string authMeEndpoint;
            if (this.defaultOptions.AuthEndpoint.StartsWith("http"))
            {
                authMeEndpoint = this.defaultOptions.AuthEndpoint; // enable pulling from places like storage account private blob container
            }
            else
            {
                authMeEndpoint = $"{uriString}/{this.defaultOptions.AuthEndpoint}"; // localhost relative path, e.g. wwwroot/.auth/me.json
            }

            var request = new HttpRequestMessage(HttpMethod.Get, authMeEndpoint);
            foreach (var header in this.Headers)
            {
                if (header.Key.StartsWith("X-ZUMO-"))
                {
                    request.Headers.Add(header.Key, header.Value[0]);
                }
            }

            return request;
        }

        private HttpClientHandler CreateHandler(ref CookieContainer cookieContainer)
        {
            var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            };
            return handler;
        }
    }
}
