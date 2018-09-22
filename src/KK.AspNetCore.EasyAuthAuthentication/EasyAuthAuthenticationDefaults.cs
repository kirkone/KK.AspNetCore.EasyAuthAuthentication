namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using Microsoft.AspNetCore.Builder;

    public static class EasyAuthAuthenticationDefaults
    {
        /// <summary>
        /// Default values related to Azure EasyAuth authentication handler
        /// </summary>
        public const string AuthenticationScheme = "EasyAuth";
        public static readonly string DisplayName = "Azure Easy Auth";
    }
}
