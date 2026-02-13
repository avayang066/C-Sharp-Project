using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Services;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        // 註冊
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("帳號與密碼不得為空");
            var success = await _service.RegisterAsync(req.Username, req.Password);
            if (!success)
                return Conflict("帳號已存在");
            return Ok(new { message = "註冊成功" });
        }

        // 登入
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("帳號與密碼不得為空");

            var (user, token) = await _service.LoginAsync(req.Username, req.Password);
            if (user == null)
                return Unauthorized("帳號或密碼錯誤");

            return Ok(
                new
                {
                    message = "登入成功",
                    token,
                    user.Id,
                    user.Username,
                }
            );
        }

        // 登出
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _service.LogoutAsync();
            return Ok(new { message = "已登出" });
        }
    }

    // 請求用 DTO
    public class UserRegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
