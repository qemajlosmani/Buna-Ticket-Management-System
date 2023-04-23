using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btms.Data.Entities
{
    [Owned]
    public class RefreshToken
    {
        [Key]
        public int id { get; set; }
        public Account user { get; set; }
        [DataType("varchar"), MaxLength(250)]
        public string? token { get; set; }
        public DateTime expires { get; set; }
        public DateTime created { get; set; }
        public DateTime? revoked { get; set; }

        [DataType("varchar"), MaxLength(250)]
        public string? replaced_by_token { get; set; }
        [DataType("varchar"), MaxLength(50)]
        public string? reason_revoked { get; set; }
        public bool is_expired => DateTime.UtcNow >= expires;
        public bool is_revoked => revoked != null;
        public bool is_active => revoked == null && !is_expired;
    }
}
