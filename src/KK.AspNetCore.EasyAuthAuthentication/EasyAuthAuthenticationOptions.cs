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
        /// A list of all provider options that should be used by easy auth.
        /// </summary>
        public IList<ProviderOptions> ProviderOptions { get; set; } = new List<ProviderOptions>();

        public LocalProviderOption? LocalProviderOption { get; set; } = null;

        /// <summary>
        /// Adds a new options object to the provider pipeline.
        /// It will replace exsisting options for the same provider. So be shure what you do.
        /// </summary>
        /// <param name="options">The provider options object with a ProviderName.</param>
        public void AddProviderOptions(ProviderOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ProviderName))
            {
                throw new ArgumentException("The ProviderName property is requiered on the ProviderOptions object.");
            }

            var exsistingProviderOption = this.ProviderOptions.FirstOrDefault(d => d.ProviderName == options.ProviderName);
            if (exsistingProviderOption == null)
            {
                this.ProviderOptions.Add(options);
            }
            else
            {
                _ = this.ProviderOptions.Remove(exsistingProviderOption);
                this.ProviderOptions.Add(options);
            }
        }
    }
}
