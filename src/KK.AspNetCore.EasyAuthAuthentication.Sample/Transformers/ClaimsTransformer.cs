namespace KK.AspNetCore.EasyAuthAuthentication.Sample.Transformers
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;

    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IMemoryCache cache;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ClaimsTransformer(
            // IRepository repository,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache
        )
        {
            // repository = repository;
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated)
            {
                var currentPrincipal = (ClaimsIdentity)principal.Identity;

                var ci = (ClaimsIdentity)principal.Identity;
                var cacheKey = ci.Name;

                if (cache.TryGetValue(cacheKey, out List<Claim> claims))
                {
                    currentPrincipal.AddClaims(claims);
                }
                else
                {
                    claims = new List<Claim>();
                    // var isUserSystemAdmin = await repository.IsUserAdmin(ci.Name);
                    // if (isUserSystemAdmin)
                    // {
                    var c = new Claim(ClaimTypes.Role, "SystemAdmin");
                    claims.Add(c);
                    // }

                    cache.Set(cacheKey, claims);
                    currentPrincipal.AddClaims(claims);
                }

                //foreach (var claim in ci.Claims)
                //{
                //    currentPrincipal.AddClaim(claim);
                //}
            }

            return await Task.FromResult(principal);
        }
    }
}