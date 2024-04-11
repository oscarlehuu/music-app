using System.Security.Claims;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using server.Service;

namespace server.Controllers
{
    [Route("/api/subscription")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly LoginService _loginService;
        public SubscriptionController(SubscriptionService subscriptionService, LoginService loginService)
        {
            _subscriptionService = subscriptionService;
            _loginService = loginService;
        }
        [HttpPost("{id}")]
        public IActionResult TestAsync(int id) 
        {
            Console.WriteLine("Test endpoint reached");
            return Ok("Test Successful: " + id);
        }
        [HttpPost("subscribe/{musicId}")]
        public async Task<IActionResult> SubscribeAsync(string musicId)
        {
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) throw new UnauthorizedAccessException();
            string userId = await _loginService.GetUserIdByEmail(email);
            Console.WriteLine("userId: " + userId);
            await _subscriptionService.SubscribeAsync(userId, musicId);
            return Ok("Subscribe successfully"); 
        }
        [HttpPost("unscribe/{musicId}")]
        public async Task<IActionResult> UnscribeAsync(string musicId)
        {
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) throw new UnauthorizedAccessException();
            string userId = await _loginService.GetUserIdByEmail(email);
            Console.WriteLine("userId: " + userId);
            await _subscriptionService.UnscribeAsync(userId, musicId);
            return Ok("Unscribe successfully");
        }
        [HttpGet]
        public async Task<IActionResult> GetSubscriptionsWithDetails()
        {
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) throw new UnauthorizedAccessException();
            string userId = await _loginService.GetUserIdByEmail(email);
            Console.WriteLine("userId: " + userId);
            var response = await _subscriptionService.GetSubscriptionsWithDetailsAsync(userId);
            return Ok(response);
        }
    }
}