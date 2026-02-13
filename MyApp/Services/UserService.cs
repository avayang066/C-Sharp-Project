using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _config;

        public UserService(ApplicationDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        // 使用者註冊
        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == username))
                return false; // 帳號已存在

            var user = new User { Username = username, PasswordHash = HashPassword(password) };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // 使用者登入
        public async Task<(User user, string token)> LoginAsync(string username, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || user.PasswordHash != HashPassword(password))
                return (default!, default!);

            var token = GenerateJwtToken(user);
            return (user, token);
        }

        // 使用者登出（僅示意，實際登出通常由前端/Token管理）
        public Task LogoutAsync()
        {
            // 若用 Cookie/Session，這裡可清除 Session
            // 若用 JWT，前端只需移除 Token
            return Task.CompletedTask;
        }

        // 密碼雜湊（SHA256）
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _config["jwtKey"];
            var jwtIssuer = _config["jwtIssuer"];
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
