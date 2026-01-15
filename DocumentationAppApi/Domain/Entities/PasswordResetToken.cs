using DocumentationApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace DocumentationAppApi.Domain.Entities
{ public class PasswordResetToken
    {
        [Key]
        public int TokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpireAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }
}
