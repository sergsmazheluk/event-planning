using Contracts;
using EventPlanning.Infrastructure.Auth;
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
        private readonly IJwtTokenService _jwt;

        public AuthController(UserManager<ApplicationUser> um, IEmailSender email, IJwtTokenService jwt)
        { 
            _um = um; 
            _email = email; 
            _jwt = jwt; 
        }

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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _um.FindByEmailAsync(dto.Email);
            if (user is null)
                return Unauthorized(new { error = "Invalid credentials" });

            // если email не подтвержден — отказ
            if (_um.Options.SignIn.RequireConfirmedEmail &&
                !await _um.IsEmailConfirmedAsync(user))
                return Unauthorized(new { error = "Email is not confirmed" });

            var passok = await _um.CheckPasswordAsync(user, dto.Password);
            if (!passok)
                return Unauthorized(new { error = "Invalid credentials" });

            var token = _jwt.Create(user);
            return Ok(new { token });
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
