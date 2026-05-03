using System.ComponentModel.DataAnnotations;

namespace CollabNest.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";

        [Required, EmailAddress(ErrorMessage = "Valid email required")]
        public string Email { get; set; } = "";

        [Required, MinLength(6, ErrorMessage = "Min 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }

    public class LoginVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public class ProfileVM
    {
        [Required]
        public string Name { get; set; } = "";
        public string Bio { get; set; } = "";
        public string Skills { get; set; } = "";
    }

    public class CreateProjectVM
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";

        public string RequiredSkills { get; set; } = "";
    }

    public class SendRequestVM
    {
        public int ProjectId { get; set; }
        public string Message { get; set; } = "";
    }

    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordVM
    {
        [Required]
        public string Token { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter a new password")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your new password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";
    }
}