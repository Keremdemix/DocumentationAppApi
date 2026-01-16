using System.ComponentModel.DataAnnotations;

namespace DocumentationAppApi.API.Models.Requests.Auth
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Mail { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string NewPasswordControl { get; set; }
    }

}
