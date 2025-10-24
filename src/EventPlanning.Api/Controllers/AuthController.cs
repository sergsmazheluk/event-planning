using EventPlanning.Infrastructure.Identity;
using EventPlanning.Infrastructure.Notifications.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Api.Controllers
{    
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _um;
        private readonly IEmailSender _email;

        public AuthController(UserManager<ApplicationUser> um, IEmailSender email)
        { _um = um; _email = email; }

        public record RegisterDto(string Email, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
            var result = await _um.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var token = await _um.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token }, Request.Scheme)!;
            await _email.SendAsync(dto.Email, "Confirm your email", $"<a href=\"{link}\">Confirm</a>");
            return Ok(new { message = "Check your email to confirm" });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var user = await _um.FindByIdAsync(userId);
            if (user is null) return NotFound();
            var result = await _um.ConfirmEmailAsync(user, token);
            return result.Succeeded ? Ok(new { message = "Email confirmed" }) : BadRequest("Invalid token");
        }
    }
}
