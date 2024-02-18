using System;

using System.ComponentModel.DataAnnotations;

namespace FirebaseLoginAuth.Models
{
    public class DataUserModel
    {
        [Required]
        public string? UserType { get; set; }

        [Required]
        public string? UserName { get; set; }

     

    }
}
