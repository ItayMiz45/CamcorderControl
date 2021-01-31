using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class User
    {
        public Int64 UserId { get; set; }
        public String Username { get; set; }

        public User(Int64 UserId, String Username)
        {
            this.UserId = UserId;
            this.Username = Username;
        }

        public User(String Username)
        {
            this.UserId = -1;
            this.Username = Username;
        }

        public override string ToString()
        {
            return $"{UserId}: {Username}";
        }
    }
}
