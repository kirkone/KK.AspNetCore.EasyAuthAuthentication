namespace KK.AspNetCore.EasyAuthAuthentication.Interfaces
{
    using KK.AspNetCore.EasyAuthAuthentication.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public interface IEasyAuthAuthentificationService
    {
        bool CanHandleAuthentification(HttpContext httpContext);

        AuthenticateResult AuthUser(HttpContext context, ProviderOptions options = null);
    }
}
