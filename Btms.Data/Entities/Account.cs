using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btms.Data.Entities
{
    public class Account
    {
        public int id { get; set; }
        [DataType("varchar"), MaxLength(10)]
        public string? title { get; set; }
        [DataType("varchar"), MaxLength(50)]
        public string? first_name { get; set; }
        [DataType("varchar"), MaxLength(50)]
        public string? last_name { get; set; }
        [DataType("varchar"), MaxLength(100)]
        public string email { get; set; }
        [DataType("varchar"), MaxLength(150)]
        public string? password { get; set; }
        public bool? accept_terms { get; set; }
        public Role role { get; set; }
        [DataType("varchar"), MaxLength(250)]
        public string? verification_token { get; set; }
        public DateTime? verified { get; set; }
        public bool is_verified => verified.HasValue || password_reset.HasValue;
        [DataType("varchar"), MaxLength(250)]
        public string? reset_token { get; set; }
        public DateTime? reset_token_expires { get; set; }
        public DateTime? password_reset { get; set; }
        public DateTime created { get; set; }
        public DateTime? updated { get; set; }

        [DefaultValue("en")]
        [DataType("varchar"), MaxLength(10)]
        public string language { get; set; }
        public List<RefreshToken> refresh_tokens { get; set; }

        public bool OwnsToken(string token)
        {
            return this.refresh_tokens?.Find(x => x.token == token) != null;
        }
    }
}
