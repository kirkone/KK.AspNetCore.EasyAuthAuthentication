namespace KK.AspNetCore.EasyAuthAuthentication.Sample.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Repository : IRepository
    {
        public Task<IEnumerable<string>> GetRoles(string userIdentifier) => Task.FromResult<IEnumerable<string>>(new[] { "SystemAdmin" });
    }
}