using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace KK.AspNetCore.EasyAuthAuthentication.Models
{
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
        public string RoleClaimType { get; set; } = ClaimTypes.Role;

        /// <summary>
        /// The provider name for this options object.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Define if this provide is active.
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
