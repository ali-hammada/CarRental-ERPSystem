using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
  public class EmployeeVM
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Full Name is required")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Password { get; set; }

    [Required(ErrorMessage = "Phone is required")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; }

    public bool IsActive { get; set; } = true;
  }
}
