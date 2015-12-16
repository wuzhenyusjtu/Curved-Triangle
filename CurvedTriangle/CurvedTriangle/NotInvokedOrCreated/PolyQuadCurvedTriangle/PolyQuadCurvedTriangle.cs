using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{

    /// <summary>
    /// We don't create any instaces of this class in this code package
    /// We use polynomial quadratic curved triangle to compare with rational quadratic curved triangle
    /// The rational ones performs better than polynomial ones
    /// </summary>
    class PolyQuadCurvedTriangle
    {
        private Position[,] sixPoles;
        private double[,] sixWeights;

        public PolyQuadCurvedTriangle()
        {
            Position[,] sixPoles = new Position[3, 3];
            double[,] sixWeights = new double[3, 3];

            sixWeights[0, 0] = 1;
            sixWeights[0, 2] = 1;
            sixWeights[2, 0] = 1;
            sixWeights[0, 1] = 1;
            sixWeights[1, 1] = 1;
            sixWeights[1, 0] = 1;
        }
    }
}
