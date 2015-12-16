using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace TestsOnModels
{
    class CubicCurvedTriangle : Triangle
    {
        protected CubicTrianglePoles cubicPoles;

        /// <summary>The vertexs of the triangle</summary>
        /// <summary> Ten control points of the curved cubic triangle </summary>
        ///<summary> Notation is from Peters/Vlachos paper </summary>
        public CubicTrianglePoles CubicPoles { get { return cubicPoles; } set { cubicPoles = value; } }

        /// <summary> Constructor for the cubic curved triangle </summary>
        /// <param name="vertices"> The vertices of the triangle template</param>
        public CubicCurvedTriangle(Vertex[] vertices)
            : base(vertices)
        {
            cubicPoles = new CubicTrianglePoles();
            GetCubicTrianglePoles();
        }

        private void GetCubicTrianglePoles()
        {
            // Three points of triangle
            Position p1 = vertexs[0].Point;
            Position p2 = vertexs[1].Point;
            Position p3 = vertexs[2].Point;


            // Three normals 
            Vector n1 = vertexs[0].Normal;
            Vector n2 = vertexs[1].Normal;
            Vector n3 = vertexs[2].Normal;

            // Interior control points of Bezier cubic edge
            Position[] controlPoints = new Position[2];

            CubicPoles.B300 = p1;    // b300 = S(0,0,1)
            CubicPoles.B030 = p2;    // b030 = S(1,0,0)
            CubicPoles.B003 = p3;    // b003 = S(0,1,0)
            controlPoints = CubicFromPtsNormals(p2, p3, n2, n3);    // Get interior control pts
            CubicPoles.B021 = controlPoints[0];
            CubicPoles.B012 = controlPoints[1];
            controlPoints = CubicFromPtsNormals(p3, p1, n3, n1);
            CubicPoles.B102 = controlPoints[0];
            CubicPoles.B201 = controlPoints[1];

            controlPoints = CubicFromPtsNormals(p1, p2, n1, n2);
            CubicPoles.B210 = controlPoints[0];
            CubicPoles.B120 = controlPoints[1];
            // Get middle pole position that gives us desired patch mid-point
            CubicPoles.B111 = GetMidPoleAlice();
        }

        private Snap.Position[] CubicFromPtsNormals(Snap.Position p0, Snap.Position p1, Snap.Vector n0, Snap.Vector n1)
        {
            // Chord vector, and its length
            Snap.Vector c = p1 - p0;
            double k = Snap.Vector.Norm(c);
            Snap.Vector w = Snap.Vector.Unit(c);

            // Tangent directions are projections of the chord onto the tangent planes.
            // Therefore each tangent lies in plane of chord and corresponding normal
            Snap.Vector v0 = c - (c * n0) * n0;
            Snap.Vector u0 = Snap.Vector.Unit(v0);
            Snap.Vector v1 = c - (c * n1) * n1;
            Snap.Vector u1 = Snap.Vector.Unit(v1);

            // Length of Bezier polygon leg -- d0 = 2k / (3*(1 + cos alpha1))
            double cosAlpha0 = w * u0;
            double cosAlpha1 = w * u1;
            double d0 = (2 * k) / (3 * (1 + cosAlpha1));
            double d1 = (2 * k) / (3 * (1 + cosAlpha0));

            // Bezier control points
            Snap.Position pa = p0 + d0 * u0;
            Snap.Position pb = p1 - d1 * u1;
            Snap.Position[] poles = new Snap.Position[2] { pa, pb };
            return poles;
        }

        /// <summary>
        /// Get the mid(#10) pole of the cubic curved triangle
        /// </summary>
        /// <param name="marginPoles">The poles on the sides of the cubic curved triangle</param>
        /// <param name="vertices"></param>
        /// <returns>The mid pole</returns>
        protected Snap.Position GetMidPoleAlice()
        {
            // Get mid-points of three cubic edges
            Position mid12 = (CubicPoles.B300 + CubicPoles.B030 + 3 * CubicPoles.B210 + 3 * CubicPoles.B120) / 8;   // mid-pt of p1-p2
            Position mid23 = (CubicPoles.B030 + CubicPoles.B003 + 3 * CubicPoles.B021 + 3 * CubicPoles.B012) / 8;   // mid-pt of p2-p3
            Position mid13 = (CubicPoles.B300 + CubicPoles.B003 + 3 * CubicPoles.B102 + 3 * CubicPoles.B201) / 8;   // mid-pt of p3-p1

            // 
            Position c1 = GetAuxPoint(CubicPoles.B300, mid23, vertexs[0].Normal);   // Circle-like curve between p1 and mid23
            Position c2 = GetAuxPoint(CubicPoles.B030, mid13, vertexs[1].Normal);
            Position c3 = GetAuxPoint(CubicPoles.B003, mid12, vertexs[2].Normal);

            // Average these three positions to get mid-point of patch (point on patch)
            Position auxPole = (c1 + c2 + c3) / 3;

            // Compute middle pole position that gives us desired patch mid-point
            Position midPole = (Position)(27 * auxPole - (CubicPoles.B300 + CubicPoles.B030 + CubicPoles.B003 + 3 * CubicPoles.B210 + 3 * CubicPoles.B120 + 3 * CubicPoles.B021 + 3 * CubicPoles.B012 + 3 * CubicPoles.B102 + 3 * CubicPoles.B201)) / 6;
            return midPole;
        }

        /// <summary>Get an auxiliary point on an arc-like curve (for mid pole of the triangle) </summary>
        /// <param name="p1"> the first position of the curve </param>
        /// <param name="p2"> the second position of the curve </param>
        /// <param name="n1"> the first normal of the curve </param>
        /// <returns>The one-third point on the arc-like curve</returns>
        private Snap.Position GetAuxPoint(Snap.Position p1, Snap.Position p2, Snap.Vector n1)
        {
            Snap.Vector p = p2 - p1;
            double w = (p2 - p1) * n1;
            double cosAlpha = Snap.Vector.Norm(Snap.Vector.Cross(p, n1)) / (Snap.Vector.Norm(p) * Snap.Vector.Norm(n1));

            // Alice's magic formula for Timmer control points
            Snap.Position pr = (((2 + 2 * cosAlpha) * cosAlpha - 1) * p1 + p2 - w * n1) / (2 * (1 + cosAlpha) * cosAlpha);
            Snap.Position ps = pr + (p2 - p1) / (1 + cosAlpha);

            // Calculate Bezier points from Timmer points 
            Snap.Position pa = (Snap.Position)(4 * pr / 3 - p1 / 3);
            Snap.Position pb = (Snap.Position)(4 * ps / 3 - p2 / 3);

            // Point at parameter value t = 1/3
            Snap.Position auxPoint = (p1 + 8 * p2 + 6 * pa + 12 * pb) / 27;

            return auxPoint;
        }
    }
}
