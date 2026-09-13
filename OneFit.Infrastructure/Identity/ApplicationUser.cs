using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OneFit.Infrastructure.Identity
{
    /// <summary>
    /// Extended Identity User with additional properties
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

