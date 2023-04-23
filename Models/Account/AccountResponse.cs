using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Account
{
    public class AccountResponse
    {
        public int id { get; set; }
        public string title { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public bool is_verified { get; set; }
        public string language { get; set; }
    }
}
