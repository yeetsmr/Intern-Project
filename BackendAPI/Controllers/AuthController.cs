using InternProject.Business;
using InternProject.Core;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/auth")] 
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var token = _authService.GenerateToken(loginDto);


            if (token == null)
            {
                return Unauthorized(new { Message = "Username or password is incorrect!" });
            }

            return Ok(new { Token = token, Message = "Login successful!" });
        }
    }
}