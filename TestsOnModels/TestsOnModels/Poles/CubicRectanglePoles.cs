using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    public class CubicRectanglePoles
    {
        private Snap.Position[,] poles = new Snap.Position[4,4];

        public Snap.Position B00 { get { return poles[0,0]; } set { poles[0,0] = value; } }
        public Snap.Position B01 { get { return poles[0,1]; } set { poles[0,1] = value; } }
        public Snap.Position B02 { get { return poles[0,2]; } set { poles[0,2] = value; } }
        public Snap.Position B03 { get { return poles[0,3]; } set { poles[0,3] = value; } }
        public Snap.Position B10 { get { return poles[1,0]; } set { poles[1,0] = value; } }
        public Snap.Position B11 { get { return poles[1,1]; } set { poles[1,1] = value; } }
        public Snap.Position B12 { get { return poles[1,2]; } set { poles[1,2] = value; } }
        public Snap.Position B13 { get { return poles[1,3]; } set { poles[1,3] = value; } }
        public Snap.Position B20 { get { return poles[2,0]; } set { poles[2,0] = value; } }
        public Snap.Position B21 { get { return poles[2,1]; } set { poles[2,1] = value; } }
        public Snap.Position B22 { get { return poles[2,2]; } set { poles[2,2] = value; } }
        public Snap.Position B23 { get { return poles[2,3]; } set { poles[2,3] = value; } }
        public Snap.Position B30 { get { return poles[3,0]; } set { poles[3,0] = value; } }
        public Snap.Position B31 { get { return poles[3,1]; } set { poles[3,1] = value; } }
        public Snap.Position B32 { get { return poles[3,2]; } set { poles[3,2] = value; } }
        public Snap.Position B33 { get { return poles[3,3]; } set { poles[3,3] = value; } }

        public Snap.Position[,] CubicRectPoles { get { return poles; } set { poles = value; } }

        public CubicRectanglePoles(Snap.Position[,] poles) 
        {
            this.poles = poles;
        }
        public CubicRectanglePoles() { }
    }
}
