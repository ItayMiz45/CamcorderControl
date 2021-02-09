using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class Gesture
    {
        public Int64 GestureId { get; set; }
        public String GestureName { get; set; }
        public Int64 HandSide { get; set; }

        public Gesture(Int64 GestureId, String GestureName, Int64 HandSide)
        {
            this.GestureId = GestureId;
            this.GestureName = GestureName;
            this.HandSide = HandSide;
        }

        public override string ToString()
        {
            string handSideStr = HandSide == 1 ? "Gav" : "Spoon";
            return $"{GestureName}: {handSideStr}";
        }
    }
}
