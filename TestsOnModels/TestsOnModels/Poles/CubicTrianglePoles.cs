using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
        ///<summary>Position b300 = S(0,0,1)</summary>
        ///<summary>Position b030 = S(1,0,0)///<summary>
        ///<summary>Position b003 = S(0,1,0)///<summary>
    public class CubicTrianglePoles
    {
        private Snap.Position[] poles = new Snap.Position[10];

        public Snap.Position B300 { get { return poles[0]; } set { poles[0] = value; } }
        public Snap.Position B210 { get { return poles[1]; } set { poles[1] = value; } }
        public Snap.Position B120 { get { return poles[2]; } set { poles[2] = value; } }
        public Snap.Position B030 { get { return poles[3]; } set { poles[3] = value; } }
        public Snap.Position B021 { get { return poles[4]; } set { poles[4] = value; } }
        public Snap.Position B012 { get { return poles[5]; } set { poles[5] = value; } }
        public Snap.Position B003 { get { return poles[6]; } set { poles[6] = value; } }
        public Snap.Position B102 { get { return poles[7]; } set { poles[7] = value; } }
        public Snap.Position B201 { get { return poles[8]; } set { poles[8] = value; } }
        public Snap.Position B111 { get { return poles[9]; } set { poles[9] = value; } }

        public Snap.Position[] CubicTriPoles { get { return poles; } set { poles = value; } }

        public CubicTrianglePoles(Snap.Position[] poles) 
        {
            this.poles = poles;
        }
        public CubicTrianglePoles(){ }
    }
}
