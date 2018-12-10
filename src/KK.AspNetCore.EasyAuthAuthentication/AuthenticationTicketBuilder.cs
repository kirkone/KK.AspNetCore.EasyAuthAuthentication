using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;

namespace KK.AspNetCore.EasyAuthAuthentication
{
    public static class AuthenticationTicketBuilder
    {
        /// <summary>
        /// Build a `AuthenticationTicket` from the given payload, the principal name and the provider name
        /// </summary>
        /// <param name="claimsPayload">A array of JObjects that have a `type` and a `val` property</param>
        /// <param name="providerName">The provider name of the current auth provider.</param>
        /// <returns>A `AuthenticationTicket`</returns>
        public static AuthenticationTicket Build(IEnumerable<JObject> claimsPayload, string providerName)
        {
            var identity = new ClaimsIdentity(
                createClaims(claimsPayload),
                // setting ClaimsIdentity.AuthenticationType to value that Azure AD non-EasyAuth setups use
                AuthenticationTypesNames.Federation
            );

            addScopeClaim(identity);
            addProviderNameClaim(identity, providerName);
            var genericPrincipal = new ClaimsPrincipal(identity);

            return new AuthenticationTicket(genericPrincipal, EasyAuthAuthenticationDefaults.AuthenticationScheme);
        }

        private static IEnumerable<Claim> createClaims(IEnumerable<JObject> claimsAsJson)
        {
            foreach (var claim in claimsAsJson)
            {
                var claimType = claim["typ"].ToString();
                switch (claimType)
                {
                    case Schemas.AuthMethod:
                        foreach (var item in claim["val"].ToString().Split(','))
                        {
                            yield return new Claim(ClaimTypes.Authentication, item);
                        }
                        break;
                    case "roles":
                        foreach (var item in claim["val"].ToString().Split(','))
                        {
                            yield return new Claim(ClaimTypes.Role, item);
                        }
                        break;
                    default:
                        yield return new Claim(claimType, claim["val"].ToString());
                        break;
                }
            }
        }

        private static void addScopeClaim(ClaimsIdentity identity)
        {
            if (!identity.Claims.Any(claim => claim.Type == "scp"))
            {
                // We are not sure if we should add this in to match what non-EasyAuth authenticated result would look like
                // with EasyAuth + Express based application configurations the scope claim will always be `user_impersonation`
                identity.AddClaim(new Claim("scp", "user_impersonation"));
            }
        }

        private static void addProviderNameClaim(ClaimsIdentity identity, string providerName)
        {
            if (!identity.Claims.Any(claim => claim.Type == "provider_name"))
            {
                identity.AddClaim(new Claim("provider_name", providerName));
            }
        }
    }
}