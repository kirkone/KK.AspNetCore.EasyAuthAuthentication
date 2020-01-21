namespace KK.AspNetCore.EasyAuthAuthentication.Models
{
    using System;

    /// <summary>
    /// All options you can set per provider.
    /// </summary>
    public class ProviderOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderOptions"/> class.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0021:Use expression body for constructors", Justification = "This syntax is better for the human eyes.")]
        public ProviderOptions(string providerName)
        {
            this.ProviderName = providerName;
        }

        /// <summary>
        /// The <c>ClaimType</c> for the Idendity User.
        /// </summary>
        /// <value>The Claim Type to use for the User. Default is <c>ClaimType</c> of the auth provider.</value>
        public string NameClaimType { get; set; }

        /// <summary>
        /// The <c>ClaimType</c> for the Idendity Role.
        /// </summary>
        /// <value>The Claim Type to use for the Roles. Default is <c>ClaimType</c> of the auth provider.</value>
        public string RoleClaimType { get; set; }

        /// <summary>
        /// The provider name for this options object.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Define if this provide is active.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// That would change the defined options of the current provider options object.
        /// </summary>
        /// <param name="options">The provider options model with the new options.</param>
        public void ChangeModel(ProviderOptions? options)
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
