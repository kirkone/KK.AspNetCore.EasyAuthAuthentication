namespace KK.AspNetCore.EasyAuthAuthentication.Sample.Transformers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using KK.AspNetCore.EasyAuthAuthentication.Sample.Repositories;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;

    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IMemoryCache cache;
        private readonly IRepository repository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ClaimsTransformer(
            IRepository repository,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache            
        )
        {
            this.repository = repository;
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)principal.Identity;                
                var userIdentifier = claimsIdentity.Name;
                List<string> roles;

                if (this.cache.TryGetValue(userIdentifier, out List<string> cachedRoles))
                {
                    roles = cachedRoles;
                }
                else
                {
                    roles = new List<string>();
                    var dbRoles = await this.repository.GetRoles(userIdentifier);

                    roles.AddRange(dbRoles);
                }

                roles.AddRange(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
                roles = roles.Distinct().ToList();
                this.cache.Set(userIdentifier, roles);

                return ( new GenericPrincipal(claimsIdentity, roles.ToArray()) );
            }

            return principal;
        }
    }
}
