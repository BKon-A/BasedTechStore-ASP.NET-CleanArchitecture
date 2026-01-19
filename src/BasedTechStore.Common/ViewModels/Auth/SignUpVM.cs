using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Common.ViewModels.Auth
{
    public class SignUpVM
    {
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Довжина паролю повинна бути не менша 6 символів")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯЇїІіЄєҐґ\s'-]+$", ErrorMessage = "Ім'я містить недопустимі символи")]
        public string FullName { get; set; } = string.Empty;
    }
}
