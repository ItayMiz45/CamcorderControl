using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class Action
    {
        public Int64 ActionId { get; set; }
        public String Command { get; set; }

        public Action(Int64 ActionId, String Command)
        {
            this.ActionId = ActionId;
            this.Command = Command;
        }
    }
}
