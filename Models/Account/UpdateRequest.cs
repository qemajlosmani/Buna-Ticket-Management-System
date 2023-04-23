using Btms.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Account
{
    public class UpdateRequest
    {
        private string _password;
        private string _confirmPassword;
        private string _role;
        private string _email;

        public string? title { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string language { get; set; }

        [EnumDataType(typeof(Role))]
        public string role
        {
            get => _role;
            set => _role = replaceEmptyWithNull(value);
        }

        [EmailAddress]
        public string email
        {
            get => _email;
            set => _email = replaceEmptyWithNull(value);
        }

        [MinLength(6)]
        public string password
        {
            get => _password;
            set => _password = replaceEmptyWithNull(value);
        }

        [Compare("password")]
        public string confirm_password
        {
            get => _confirmPassword;
            set => _confirmPassword = replaceEmptyWithNull(value);
        }

        private string replaceEmptyWithNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
