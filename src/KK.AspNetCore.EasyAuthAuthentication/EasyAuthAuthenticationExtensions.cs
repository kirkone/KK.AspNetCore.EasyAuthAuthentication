namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class EasyAuthAuthenticationExtensions
    {
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder)
            => builder.AddEasyAuth(EasyAuthAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddEasyAuth(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, Action<EasyAuthAuthenticationOptions> configureOptions)
            => builder.AddEasyAuth(EasyAuthAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme, Action<EasyAuthAuthenticationOptions> configureOptions)
            => builder.AddEasyAuth(authenticationScheme, displayName: EasyAuthAuthenticationDefaults.DisplayName, configureOptions: configureOptions);

        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<EasyAuthAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<EasyAuthAuthenticationOptions, EasyAuthAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
