using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Account
{
    public class ResetPasswordRequest
    {
        [Required]
        public string token { get; set; }

        [Required]
        [MinLength(6)]
        public string password { get; set; }

        [Required]
        [Compare("password")]
        public string confirm_password { get; set; }
    }
}
