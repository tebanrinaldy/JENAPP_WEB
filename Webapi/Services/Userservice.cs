using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Webapi.Data;
using Webapi.Models;
using Webapi.Repositories;

namespace Webapi.Services
{
    public class Userservice
    {
        private readonly IRepository<User> _repository;
        public Userservice(IRepository<User> repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _repository.GetAllAsync();
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async  Task<string> RegisterUser(User user)
        {
            var users = await _repository.GetAllAsync();
            var existingUser = users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
                return "El usuario ya existe";

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _repository.AddAsync(user);
            return "Usuario registrado con éxito";

        }
        public async Task UpdateUserAsync(User user)
        {
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }
            await _repository.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
        public async Task<User?> ValidateUser(string Username, string Password)
        {
            var users = await _repository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == Username);

            if (user == null)
                return null;

            bool valid = BCrypt.Net.BCrypt.Verify(Password, user.Password);
            return valid ? user : null;
        }

    }
}