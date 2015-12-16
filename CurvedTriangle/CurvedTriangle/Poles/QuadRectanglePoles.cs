using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurvedTriangle
{
    public class QuadRectanglePoles
    {
        private Snap.Position[,] points = new Snap.Position[3,3];
        private double[,] weights = new double[3,3];

        public Snap.Position P00 { get { return points[0,0]; } set { points[0,0] = value; } }
        public Snap.Position P01 { get { return points[0,1]; } set { points[0,1] = value; } }
        public Snap.Position P02 { get { return points[0,2]; } set { points[0,2] = value; } }
        public Snap.Position P10 { get { return points[1,0]; } set { points[1,0] = value; } }
        public Snap.Position P11 { get { return points[1,1]; } set { points[1,1] = value; } }
        public Snap.Position P12 { get { return points[1,2]; } set { points[1,2] = value; } }
        public Snap.Position P20 { get { return points[2,0]; } set { points[2,0] = value; } }
        public Snap.Position P21 { get { return points[2,1]; } set { points[2,1] = value; } }
        public Snap.Position P22 { get { return points[2,2]; } set { points[2,2] = value; } }


        public double W00 { get { return weights[0, 0]; } set { weights[0, 0] = value; } }
        public double W01 { get { return weights[0, 1]; } set { weights[0, 1] = value; } }
        public double W02 { get { return weights[0, 2]; } set { weights[0, 2] = value; } }
        public double W10 { get { return weights[1, 0]; } set { weights[1, 0] = value; } }
        public double W11 { get { return weights[1, 1]; } set { weights[1, 1] = value; } }
        public double W12 { get { return weights[1, 2]; } set { weights[1, 2] = value; } }
        public double W20 { get { return weights[2, 0]; } set { weights[2, 0] = value; } }
        public double W21 { get { return weights[2, 1]; } set { weights[2, 1] = value; } }
        public double W22 { get { return weights[2, 2]; } set { weights[2, 2] = value; } }


        public Snap.Position[,] QuadRectPoints { get { return points; } set { points = value; } }
        public double[,] QuadRectWeights { get { return weights; } set { weights = value; } }

        public QuadRectanglePoles(Snap.Position[,] points) 
        {
            this.points = points;
        }
        public QuadRectanglePoles() { }
    }
}
 