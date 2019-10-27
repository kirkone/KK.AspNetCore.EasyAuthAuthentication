namespace KK.AspNetCore.EasyAuthAuthentication.Samples.Web.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository
    {
        Task<IEnumerable<string>> GetRoles(string userIdentifier);
    }
}