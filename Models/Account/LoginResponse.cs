using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.Account
{
    public class LoginResponse
    {
        public int id { get; set; }
        public string title { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        [JsonIgnore]
        public DateTime created { get; set; }
        [JsonIgnore]
        public DateTime? updated { get; set; }
        [JsonIgnore]
        public bool is_verified { get; set; }
        public string access_token { get; set; }
        public string language { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string refresh_token { get; set; }
    }
}
