using Microsoft.AspNetCore.Identity;

namespace authService.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
