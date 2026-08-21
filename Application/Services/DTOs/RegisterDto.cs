using System.ComponentModel.DataAnnotations;

namespace Application.Services.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;

        [Required, Phone]
        public string Phone { get; set; } = null!;

        public string Role { get; set; } = "Employee";
    }
}
