using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    public class QuadTrianglePoles
    {
        private Snap.Position[] points = new Snap.Position[6];
        private double[] weights = new double[6];
        public Snap.Position P00 { get { return points[0]; } set { points[0] = value; } }
        public Snap.Position P10 { get { return points[1]; } set { points[1] = value; } }
        public Snap.Position P20 { get { return points[2]; } set { points[2] = value; } }
        public Snap.Position P11 { get { return points[3]; } set { points[3] = value; } }
        public Snap.Position P02 { get { return points[4]; } set { points[4] = value; } }
        public Snap.Position P01 { get { return points[5]; } set { points[5] = value; } }

        public double W00 { get { return weights[0]; } set { weights[0] = value; } }
        public double W10 { get { return weights[1]; } set { weights[1] = value; } }
        public double W20 { get { return weights[2]; } set { weights[2] = value; } }
        public double W11 { get { return weights[3]; } set { weights[3] = value; } }
        public double W02 { get { return weights[4]; } set { weights[4] = value; } }
        public double W01 { get { return weights[5]; } set { weights[5] = value; } }

        public Snap.Position[] QuadTriPoles { get { return points; } set { points = value; } }
        public double[] QuadTriWeights { get { return weights; } set { weights = value; } }

        public QuadTrianglePoles(Snap.Position[] points) 
        {
            this.points = points;
        }
        public QuadTrianglePoles(){ }
    }
}
