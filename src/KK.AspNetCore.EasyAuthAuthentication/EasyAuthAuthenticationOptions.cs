namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using Microsoft.AspNetCore.Authentication;

    public class EasyAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        private string authEndpoint = ".auth/me";
        public string AuthEndpoint { get => authEndpoint; set => authEndpoint = value; }

        public EasyAuthAuthenticationOptions()
        {

        }
    }
}
