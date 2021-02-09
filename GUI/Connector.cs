using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class Connector
    {
        public Int64 ConnectorId { get; set; }
        public Int64 UserId { get; set; }
        public List<int> GesturesArray { get; set; }
        public List<int> ActionsArray { get; set; }

        public Connector(Int64 ConnectorId, Int64 UserId, String GesturesArray, String ActionsArray)
        {
            this.ConnectorId = ConnectorId;
            this.UserId = UserId;

            this.GesturesArray = GesturesArray.Split(',').ToList<String>().Select(int.Parse).ToList();
            this.ActionsArray = ActionsArray.Split(',').ToList<String>().Select(int.Parse).ToList();
        }
    }
}
