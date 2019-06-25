namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Options for the <see cref="EasyAuthAuthenticationHandler"/>.
    /// </summary>
    public class EasyAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The endpoint where to look for the <c>JSON</c> with the authentification information.
        /// </summary>
        /// <value>A relative path to the <c>wwwroot</c> folder.</value>
        public string AuthEndpoint { get; set; } = ".auth/me";

        /// <summary>
        /// The <c>ClaimType</c> for the Idendity User.
        /// </summary>
        /// <value>The Claim Type to use for the User. Default is <c>ClaimTypes.Name</c>.</value>
        public string NameClaimType { get; set; } = ClaimTypes.Name;

        /// <summary>
        /// The <c>ClaimType</c> for the Idendity Role.
        /// </summary>
        /// <value>The Claim Type to use for the Roles. Default is <c>ClaimTypes.Role</c>.</value>
        public string RoleClaimType { get; set; } = ClaimTypes.Role;
    }
}
