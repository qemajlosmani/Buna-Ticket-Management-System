using Btms.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Account
{
    public class CreateAccountRequest
    {
        public string? title { get; set; }
        public string? first_name { get; set; }

        public string? last_name { get; set; }

        [Required]
        [EnumDataType(typeof(Role))]
        public string role { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }
        public string? language { get; set; }

        [Required]
        [MinLength(6)]
        public string password { get; set; }

        [Required]
        [Compare("password")]
        public string confirm_password { get; set; }
    }
}
