using System.Threading.Tasks;
using MyApp.Models;

namespace MyApp.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string username, string password);
        Task<(User user, string token)> LoginAsync(string username, string password);
        Task LogoutAsync();
    }
}