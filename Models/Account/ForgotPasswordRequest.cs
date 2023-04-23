using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Account
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }
    }
}
