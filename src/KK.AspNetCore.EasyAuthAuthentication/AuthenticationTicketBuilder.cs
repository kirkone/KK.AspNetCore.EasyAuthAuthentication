namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Newtonsoft.Json.Linq;

    internal static class AuthenticationTicketBuilder
    {
        /// <summary>
        /// Build a `AuthenticationTicket` from the given payload, the principal name and the provider name.
        /// </summary>
        /// <param name="claimsPayload">A array of JObjects that have a `type` and a `val` property.</param>
        /// <param name="providerName">The provider name of the current auth provider.</param>
        /// <param name="options">The <c>EasyAuthAuthenticationOptions</c> to use.</param>
        /// <returns>A `AuthenticationTicket`.</returns>
        public static AuthenticationTicket Build(IEnumerable<JObject> claimsPayload, string providerName, EasyAuthAuthenticationOptions options)
        {
            // setting ClaimsIdentity.AuthenticationType to value that Azure AD non-EasyAuth setups use
            var identity = new ClaimsIdentity(
                CreateClaims(claimsPayload),
                AuthenticationTypesNames.Federation,
                options.NameClaimType,
                options.RoleClaimType
            );

            AddScopeClaim(identity);
            AddProviderNameClaim(identity, providerName);
            var genericPrincipal = new ClaimsPrincipal(identity);

            return new AuthenticationTicket(genericPrincipal, EasyAuthAuthenticationDefaults.AuthenticationScheme);
        }

        private static IEnumerable<Claim> CreateClaims(IEnumerable<JObject> claimsAsJson)
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

        private static void AddScopeClaim(ClaimsIdentity identity)
        {
            if (!identity.Claims.Any(claim => claim.Type == "scp"))
            {
                // We are not sure if we should add this in to match what non-EasyAuth authenticated result would look like
                // with EasyAuth + Express based application configurations the scope claim will always be `user_impersonation`
                identity.AddClaim(new Claim("scp", "user_impersonation"));
            }
        }

        private static void AddProviderNameClaim(ClaimsIdentity identity, string providerName)
        {
            if (!identity.Claims.Any(claim => claim.Type == "provider_name"))
            {
                identity.AddClaim(new Claim("provider_name", providerName));
            }
        }
        
        private static void AddUserIdClaim(ClaimsIdentity identity, string claimType, string userid)
        {
            if (!identity.Claims.Any(claim => claim.Type == claimType))
            {
                identity.AddClaim(new Claim(claimType, userid));
            }
        }
    }
}
