namespace KK.AspNetCore.EasyAuthAuthentication.Interfaces
{
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using KK.AspNetCore.EasyAuthAuthentication.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A service that can be used as authentification service in the <see cref="EasyAuthAuthenticationHandler"/>.
    /// </summary>
    public interface IEasyAuthAuthentificationService
    {
        /// <summary>
        /// Define if this service can handle the authentification.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> of the http request.</param>
        /// <returns>If the return is true the service can be used for this request.</returns>
        bool CanHandleAuthentification(HttpContext httpContext);

        /// <summary>
        /// Try to create a <see cref="AuthenticateResult"/> out of the <see cref="HttpContext"/> from the incomming request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> of the http request.</param>        
        /// <returns>If the user can be authentificated it will returend a full <see cref="AuthenticateResult"/>. If not this will return a <see cref="AuthenticateResult.Fail(string)"/> result. (only the <see cref="LocalAuthMeService"/> can return this.</returns>
        AuthenticateResult AuthUser(HttpContext context);

        /// <summary>
        /// Try to create a <see cref="AuthenticateResult"/> out of the <see cref="HttpContext"/> from the incomming request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> of the http request.</param>
        /// <param name="options">The <see cref="ProviderOptions"/> that can change the behavior of the <see cref="AuthUser(HttpContext, ProviderOptions)"/> method.</param>
        /// <returns>If the user can be authentificated it will returend a full <see cref="AuthenticateResult"/>. If not this will return a <see cref="AuthenticateResult.Fail(string)"/> result. (only the <see cref="LocalAuthMeService"/> can return this.</returns>
        AuthenticateResult AuthUser(HttpContext context, ProviderOptions options);
    }
}
