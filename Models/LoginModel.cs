using System.ComponentModel.DataAnnotations;

namespace FirebaseLoginAuth.Models
{
    public class LoginModel
    {

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }

  
        public string? UserType { get; set; }

        public string? UserName { get; set; }
    }
}
