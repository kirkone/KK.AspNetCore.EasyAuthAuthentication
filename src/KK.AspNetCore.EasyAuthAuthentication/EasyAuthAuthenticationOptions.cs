namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
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
    }
}
