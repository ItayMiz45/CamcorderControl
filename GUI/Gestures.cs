using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    class Gestures
    {
        public Int64 Gestureid;
        public string GestureName;
        public Int64 GestureSide;

        public Gestures(Int64 Gestureid, String GestureName, Int64 GestureSide)
        {
            this.Gestureid = Gestureid;
            this.GestureName = GestureName;
            this.GestureSide = GestureSide;
        }

        public Gestures(string GestureName, Int64 GestureSide)
        {
            this.Gestureid = -1;
            this.GestureName = GestureName;
            this.GestureSide = GestureSide;
        }

    }
}
