namespace FinanceManagerAPI.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Helpers;
    using DAL;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class UserService : IUserService
    {
        private readonly IDataRepository<User> _userRepository;

        public UserService(IDataRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = await _userRepository.GetByProperty(nameof(User.Username), username);
            if (user == null)
            {
                return null;
            }

            return PasswordHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt) 
                ? user
                : null;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _userRepository.Get().ToListAsync();
        }

        public async Task<User> GetById(int id)
        {
            return await _userRepository.GetById(id);
        }

        public async Task<User> Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new AppException("Password is required");
            }

            if (await _userRepository.EntityExists(nameof(User.Username), user.Username))
            {
                throw new AppException("Username \"" + user.Username + "\" is already taken");
            }

            PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _userRepository.Add(user);
            
            return await _userRepository.SaveAsync(user);
        }

        public async Task<User> Update(User userParam, string password = null)
        {
            var user = await _userRepository.GetById(userParam.Id);

            if (user == null)
                throw new AppException("User not found");

            if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
            {
                if (await _userRepository.EntityExists(nameof(User.Username), user.Username))
                {
                    throw new AppException("Username \"" + user.Username + "\" is already taken");
                }

                user.Username = userParam.Username;
            }

            if (!string.IsNullOrWhiteSpace(userParam.FirstName))
            {
                user.FirstName = userParam.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(userParam.LastName))
            {
                user.LastName = userParam.LastName;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                PasswordHelper.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }
            
            _userRepository.Update(user);
            return await _userRepository.SaveAsync(user);
        }

        public async Task Delete(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user != null)
            {
                _userRepository.Delete(user);
                await _userRepository.SaveAsync(user);
            }
        }
    }
}