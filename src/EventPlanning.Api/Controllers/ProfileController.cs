using Contracts.Profiles;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public sealed class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        public ProfileController(UserManager<ApplicationUser> users) => _users = users;

        [HttpGet("me")]
        public async Task<ActionResult<ProfileDto>> Me()
        {
            var user = await _users.GetUserAsync(User);
            if (user is null) return Unauthorized();
            return Ok(new ProfileDto(user.ProfileCompleted, user.ExtraProfileJson));
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromBody] CompleteProfileRequest req)
        {
            var user = await _users.GetUserAsync(User);
            if (user is null) return Unauthorized();

            user.ExtraProfileJson = req.ExtraProfileJson;
            user.ProfileCompleted = true;

            var res = await _users.UpdateAsync(user);
            if (!res.Succeeded) return Problem(string.Join(";", res.Errors.Select(e => e.Description)));
            return Ok(new { status = "completed" });
        }
    }
}
