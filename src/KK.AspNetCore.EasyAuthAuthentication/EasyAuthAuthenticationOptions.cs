namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using Microsoft.AspNetCore.Authentication;

    public class EasyAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string AuthEndpoint { get; set; } = ".auth/me";
    }
}
