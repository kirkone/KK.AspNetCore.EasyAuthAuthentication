namespace KK.AspNetCore.EasyAuthAuthentication.Interfaces
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public interface IEasyAuthAuthentificationService
    {
        bool CanHandleAuthentification(HttpContext httpContext);

        AuthenticateResult AuthUser(HttpContext context, EasyAuthAuthenticationOptions options = null);
    }
}
