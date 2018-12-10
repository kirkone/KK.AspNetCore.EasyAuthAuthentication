using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KK.AspNetCore.EasyAuthAuthentication.Services
{
    public class EasyAuthWithAuthMeService
    {
        private string Host { get; }
        private IRequestCookieCollection Cookies { get; }
        private IHeaderDictionary Headers { get; }
        private string AuthEndPoint { get; }
        private ILogger Logger { get; }
        private string HttpSchema { get; }
        private EasyAuthWithAuthMeService(
            ILogger logger,
            string httpSchema,
            string host,
            IRequestCookieCollection cookies,
            IHeaderDictionary headers,
            string authEndPoint)
        {
            this.HttpSchema = httpSchema;
            this.Host = host;
            Cookies = cookies;
            Headers = headers;
            AuthEndPoint = authEndPoint;
            this.Logger = logger;

        }

        /// <summary>
        /// Use this method to authenticate a user with easy auth.
        /// This will set the `context.User` of your HttpContext.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context">The http context with the missing user claim.</param>
        /// <param name="authEndpoint">The auth endpoint where we find the easy auth json</param>
        /// <returns></returns>
        public static async Task<AuthenticateResult> AuthUser(ILogger logger, HttpContext context, string authEndpoint)
        {
            try
            {
                var authService = new EasyAuthWithAuthMeService(
                    logger,
                    context.Request.Scheme,
                    context.Request.Host.ToString(),
                    context.Request.Cookies,
                    context.Request.Headers,
                    authEndpoint);

                var ticket = await authService.CreateUserTicket();
                logger.LogInformation("Set identity to user context object.");
                context.User = ticket.Principal;
                logger.LogInformation("identity build was a success, returning ticket");
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

            JArray payload = null;
            payload = await this.GetAuthMe(handler, httpRequest);

            // build up identity from json...
            var ticket = this.BuildIdentityFromEasyAuthMeJson((JObject)payload[0]);

            this.Logger.LogInformation("Set identity to user context object.");
            return ticket;
        }

        private AuthenticationTicket BuildIdentityFromEasyAuthMeJson(JObject payload)
        {
            var name = payload["user_id"].Value<string>(); // X-MS-CLIENT-PRINCIPAL-NAME
            this.Logger.LogDebug($"payload was fetched from easyauth me json, name: {name}");

            var identity = new GenericIdentity(name, AuthenticationTypesNames.Federation); // setting ClaimsIdentity.AuthenticationType to value that azuread non-easyauth setups use

            this.Logger.LogInformation("building claims from payload...");
            var providerName = payload["provider_name"].Value<string>();
            return AuthenticationTicketBuilder.Build(payload["user_claims"].Children<JObject>(), name, providerName);
        }

        private async Task<JArray> GetAuthMe(HttpClientHandler handler, HttpRequestMessage httpRequest)
        {
            JArray payload = null;
            using (var client = new HttpClient(handler))
            {
                var response = await client.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    this.Logger.LogDebug("auth endpoint was not successful. Status code: {0}, reason {1}", response.StatusCode, response.ReasonPhrase);
                    throw new WebException("Unable to fetch user information from auth endpoint.");
                }

                var content = await response.Content.ReadAsStringAsync();
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
            this.Logger.LogInformation($"identity not found, attempting to fetch from auth endpoint '/{this.AuthEndPoint}'");

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
            var authMeEndpoint = string.Empty;
            if (this.AuthEndPoint.StartsWith("http")) authMeEndpoint = this.AuthEndPoint; // enable pulling from places like storage account private blob container
            else authMeEndpoint = $"{uriString}/{this.AuthEndPoint}"; // localhost relative path, e.g. wwwroot/.auth/me.json

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