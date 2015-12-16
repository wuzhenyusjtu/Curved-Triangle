using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurvedTriangle
{
    /// <summary>
    /// We don't create instances of this class in this code package
    /// It provides a method to get the middle pole, which is different from Alice's method
    /// In comparison, Please see the GetMidPoleAlice private method in CubicCurvedTriangle class
    /// </summary>
    class GetMidPolePN
    {
        /// <summary>
        /// Get the mid(#10) pole of the cubic curved triangle, use the formula on PN triangle paper
        /// </summary>
        /// <param name="marginPoles">The poles on the sides of the cubic curved triangle</param>
        /// <param name="vertexs"></param>
        /// <returns>The mid pole</returns>
        public Snap.Position GetMidPole(CubicTrianglePoles CubicPoles, Vertex[] vertexs)
        {
            Snap.Position E = (CubicPoles.B210 + CubicPoles.B120 + CubicPoles.B021 + CubicPoles.B012 + CubicPoles.B102 + CubicPoles.B201) / 6;
            Snap.Position V = (CubicPoles.B300 + CubicPoles.B030 + CubicPoles.B003) / 3;
            Snap.Position midPole = E + (E - V) / 2;
            //Snap.InfoWindow.WriteLine("(" + midPole.X.ToString() + "," + midPole.Y.ToString() + "," + midPole.Z.ToString() + ")");
            return midPole;
        }
    }
}
