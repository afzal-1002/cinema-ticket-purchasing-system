using CinemaTicket.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaTicket.Services
{
    public interface IUserService
    {
        // ✅ Authentication
        Task<User?> AuthenticateAsync(string username, string password);
        
        // ✅ CRUD Operations
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);  // ✅ Single parameter
        Task<bool> DeleteUserAsync(int id);
        
        // ✅ Query Operations
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetAdminUsersAsync();
        
        // ✅ Validation Helpers
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}