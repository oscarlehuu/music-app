using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using server.Service;

namespace server.Controllers
{
    [Route("/api/authenticate")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private LoginService _loginService;
        private IConfiguration _iConfiguration;
        public LoginController(LoginService loginService, IConfiguration iConfiguration) 
        {
            _loginService = loginService;
            _iConfiguration = iConfiguration;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Ok(new { isValid = true }); 
            }
            else
            {
                return Unauthorized(new { isValid = false });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> ValidateLogin([FromBody] LoginRequest request)
        {
            if (!HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {       
                HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*"); // Or your specific frontend origin
                HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "OPTIONS,POST");
                HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
            }
            if (HttpContext.Request.Method == "OPTIONS") 
            {
                return Ok();
            }
            string token = await _loginService.ValidateLogin(request.Email, request.Password);
            if (token != null) 
            {
                return Ok(new { success = true, token = token});
            } else 
            {
                return BadRequest(new { success = false, message = "Invalid email or password" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterLogin(DTO.RegisterRequest request)
        {
            var registerResult = await _loginService.RegisterUserAsync(request);

            if (registerResult.Success)
            {
                return Ok(registerResult.Message);
            }
            else
            {
                return BadRequest(registerResult.Message);
            }
        }
    }
}