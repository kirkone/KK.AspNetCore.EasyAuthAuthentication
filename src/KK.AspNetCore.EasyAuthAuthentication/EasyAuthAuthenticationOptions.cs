namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using KK.AspNetCore.EasyAuthAuthentication.Models;
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

        private IList<ProviderOptions> providerOptions = new List<ProviderOptions>();

        public IEnumerable<ProviderOptions> ProviderSettings => this.providerOptions;

        /// <summary>
        /// Adds a new options object to the provider pipeline.
        /// It will replace exsisting options for the same provider. So be shure what you do.
        /// </summary>
        /// <param name="options">The provider options object with a ProviderName</param>
        public void AddProviderOptions(ProviderOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ProviderName))
            {
                throw new ArgumentException("The ProviderName property is requiered on the ProviderOptions object.");
            }
            var exsistingProviderOption = this.providerOptions.FirstOrDefault(d => d.ProviderName == options.ProviderName);
            if (exsistingProviderOption == null)
            {
                this.providerOptions.Add(options);
            }
            else
            {
                this.providerOptions.Remove(exsistingProviderOption);
                this.providerOptions.Add(options);
            }
        }

    }
}
