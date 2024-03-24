using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [HttpPost("login")]
        public async Task<IActionResult> ValidateLogin(LoginRequest request)
        {
            string token = await _loginService.ValidateLogin(request.Email, request.Password);
            if (token != null) 
            {
                return Ok(new { success = true, token = token});
            } else 
            {
                return BadRequest(new { success = false, message = "Invalid email or password" });
            }
        }
    }
}