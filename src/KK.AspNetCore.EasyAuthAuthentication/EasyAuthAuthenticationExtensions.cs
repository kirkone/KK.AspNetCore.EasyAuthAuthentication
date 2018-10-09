namespace KK.AspNetCore.EasyAuthAuthentication
{
    using System;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Extension methods for <see cref="AuthenticationBuilder"/> to add the <see cref="EasyAuthAuthenticationHandler"/> to the pipeline.
    /// </summary>
    public static class EasyAuthAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder)
            => builder.AddEasyAuth(EasyAuthAuthenticationDefaults.AuthenticationScheme);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="authenticationScheme">The schema for the Easy Auth handler.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddEasyAuth(authenticationScheme, configureOptions: null);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="configureOptions">A callback to configure <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, Action<EasyAuthAuthenticationOptions> configureOptions)
            => builder.AddEasyAuth(EasyAuthAuthenticationDefaults.AuthenticationScheme, configureOptions);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="authenticationScheme">The schema for the Easy Auth handler.</param>
        /// <param name="configureOptions">A callback to configure <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme, Action<EasyAuthAuthenticationOptions> configureOptions)
            => builder.AddEasyAuth(authenticationScheme, displayName: EasyAuthAuthenticationDefaults.DisplayName, configureOptions: configureOptions);

        /// <summary>
        /// Adds the <see cref="EasyAuthAuthenticationHandler"/> for authentication.
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        /// <param name="authenticationScheme">The schema for the Easy Auth handler.</param>
        /// <param name="displayName">The display name for the Easy Auth handler.</param>
        /// <param name="configureOptions">A callback to configure <see cref="EasyAuthAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddEasyAuth(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<EasyAuthAuthenticationOptions> configureOptions)
            => builder.AddScheme<EasyAuthAuthenticationOptions, EasyAuthAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
    }
}
