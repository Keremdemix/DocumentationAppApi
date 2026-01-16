using System.ComponentModel.DataAnnotations;

namespace DocumentationAppApi.API.Models.Requests.Auth
{
    public class ResetTokenRequest
    {
        [Required]
        [EmailAddress]
        public required string Mail { get; set; }
    }
}
