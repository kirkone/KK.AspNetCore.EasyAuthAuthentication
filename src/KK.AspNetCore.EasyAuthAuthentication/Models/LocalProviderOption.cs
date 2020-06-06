namespace KK.AspNetCore.EasyAuthAuthentication.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KK.AspNetCore.EasyAuthAuthentication.Services;

    public class LocalProviderOption
    {
        public LocalProviderOption()
        {
            this.AuthEndpoint = string.Empty;
            this.NameClaimType =string.Empty;
            this.RoleClaimType = string.Empty;
        }
        public LocalProviderOption(string authEndpoint, string nameClaimType, string roleClaimType)
        {
            this.AuthEndpoint = authEndpoint;
            this.NameClaimType = nameClaimType;
            this.RoleClaimType = roleClaimType;
        }
        /// <summary>
        /// The endpoint where to look for the <c>JSON</c> with the authentification information.
        /// </summary>
        /// <value>A relative path to the <c>wwwroot</c> folder.</value>
        public string AuthEndpoint { get; set; }
        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }

        /// <summary>
        /// That would change the defined options of the current provider options object.
        /// </summary>
        /// <param name="options">The provider options model with the new options.</param>
        public void ChangeModel(LocalProviderOption? options)
        {
            if (options == null)
            {
                return;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(options.NameClaimType))
                {
                    this.NameClaimType = options.NameClaimType;
                }

                if (!string.IsNullOrWhiteSpace(options.RoleClaimType))
                {
                    this.RoleClaimType = options.RoleClaimType;
                }

                if (!string.IsNullOrWhiteSpace(options.AuthEndpoint))
                {
                    this.AuthEndpoint = options.AuthEndpoint;
                }
            }
        }

        public ProviderOptions GetProviderOptions() => new ProviderOptions(typeof(LocalAuthMeService).Name, this.NameClaimType, this.RoleClaimType);
    }
}
