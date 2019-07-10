namespace KK.AspNetCore.EasyAuthAuthentication.Models
{
    using System;
    using System.Security.Claims;

    public class ProviderOptions
    {
        public ProviderOptions(string providerName)
        {
            this.ProviderName = providerName;
        }

        /// <summary>
        /// The <c>ClaimType</c> for the Idendity User.
        /// </summary>
        /// <value>The Claim Type to use for the User. Default is <c>ClaimTypes.Name</c>.</value>
        public string NameClaimType { get; set; }

        /// <summary>
        /// The <c>ClaimType</c> for the Idendity Role.
        /// </summary>
        /// <value>The Claim Type to use for the Roles. Default is <c>ClaimTypes.Role</c>.</value>
        public string RoleClaimType { get; set; }

        /// <summary>
        /// The provider name for this options object.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Define if this provide is active.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// That would change the defined options of the current provider options object.
        /// </summary>
        /// <param name="options">The provider options model with the new options</param>
        public void ChangeModel(ProviderOptions options)
        {
            if (options == null)
            {
                return;
            }
            else if (options.ProviderName != this.ProviderName)
            {
                throw new ArgumentException("You can only use the method ChangeModel if you use the same provider name.");
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

                this.Enabled = options.Enabled;
            }
        }
    }
}
