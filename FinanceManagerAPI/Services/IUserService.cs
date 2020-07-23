namespace FinanceManagerAPI.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        
        Task<IEnumerable<User>> GetAll();

        Task<User> GetById(int id);

        Task<User> Create(User user, string password);

        Task<User> Update(User user, string password = null);
        
        Task Delete(int id);
    }
}