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
        /// <param name="principalName">The principal name of the user.</param>
        /// /// <param name="providerName">The provider name of the current auth provider.</param>
        /// <returns>A `AuthenticationTicket`</returns>
        public static AuthenticationTicket Build(IEnumerable<JObject> claimsPayload, string principalName, string providerName)
        {
            var identity = new GenericIdentity(principalName, AuthenticationTypesNames.Federation); // setting ClaimsIdentity.AuthenticationType to value that azuread non-easyauth setups use
            identity.AddClaims(createClaims(claimsPayload));
            addScpClaim(identity);
            identity.AddClaim(new Claim("provider_name", providerName));
            var genericPrincipal = new GenericPrincipal(identity, null);
            return new AuthenticationTicket(genericPrincipal, EasyAuthAuthenticationDefaults.AuthenticationScheme);
        }

        private static IEnumerable<JObject> getTheClaimsNodeFromPayload(JObject payload)
        {
            return payload["user_claims"].Children<JObject>();
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

        private static void addScpClaim(ClaimsIdentity identity)
        {
            if (!identity.Claims.Any(claim => claim.Type == "scp"))
            {
                identity.AddClaim(new Claim("scp", "user_impersonation")); // not sure why easyauth is dropping this
            }
        }
    }
}