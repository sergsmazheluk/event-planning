using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Infrastructure.Identity
{
    public sealed class ApplicationUser : IdentityUser
    {
        public bool ProfileCompleted { get; set; }
        public string? ExtraProfileJson { get; set; }
    }
}
