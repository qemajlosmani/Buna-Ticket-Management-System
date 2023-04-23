using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Account
{
    public class VerifyEmailRequest
    {
        [Required]
        public string token { get; set; }
        [Required]
        public string password { get; set; }
    }
}
