using System.ComponentModel.DataAnnotations;

namespace OneFit.Application.Features.Authentication.DTOs
{
    /// <summary>
    /// Request model for user registration
    /// </summary>
    public class RegisterUserRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "First Name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "Last Name must be between 2 and 50 characters")]
        public string LastName { get; set; } = string.Empty;
    }
}