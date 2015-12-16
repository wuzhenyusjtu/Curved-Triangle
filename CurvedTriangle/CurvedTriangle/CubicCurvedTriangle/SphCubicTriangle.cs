using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
   /// <summary>
   /// Get cubic Bezier triangular patch that lies on a sphere (default case)
   /// </summary>
   /// <remarks>
   /// We use this code if we don't know the type of the face (e.g. cylindrical)
   /// </remarks>
    class SphCubicTriangle : CubicCurvedTriangle
    {
        public SphCubicTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID, NXOpen.Facet.FacetedFace.FacetedfaceType faceType)
            : base(vertexs, sides, facetID, faceID, faceType)
        {
            cubicPoles = new CubicTrianglePoles();
            GetCubicTrianglePoles();
        }

        /// <summary>Get the ten control points of the cubic triangular Bezier patch </summary>
        /// <returns> ten control points of a triangular cubic Bezier patch</returns>
        protected override void GetCubicTrianglePoles()
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

            // Construct edge between p2 and p3
            if (Sides[0].IsOnEdge == false)
            {
                controlPoints = CubicFromPtsNormals(p2, p3, n2, n3);    // Get interior control pts
                CubicPoles.B021 = controlPoints[0];
                CubicPoles.B012 = controlPoints[1];
            }
            else
            {
                controlPoints = CubicFromTangents(p2, p3, Sides[0].Tangent0, Sides[0].Tangent1);
                CubicPoles.B021 = controlPoints[0];
                CubicPoles.B012 = controlPoints[1];
            }

            // Construct edge between p1 and p3            
            if (Sides[1].IsOnEdge == false)
            {
                controlPoints = CubicFromPtsNormals(p3, p1, n3, n1);
                CubicPoles.B102 = controlPoints[0];
                CubicPoles.B201 = controlPoints[1];
            }
            else
            {
                controlPoints = CubicFromTangents(p3, p1, Sides[1].Tangent0, Sides[1].Tangent1);
                CubicPoles.B102 = controlPoints[0];
                CubicPoles.B201 = controlPoints[1];
            }

            // Construct edge between p1 and p2 
            if (Sides[2].IsOnEdge == false)
            {
                controlPoints = CubicFromPtsNormals(p1, p2, n1, n2);
                CubicPoles.B210 = controlPoints[0];
                CubicPoles.B120 = controlPoints[1];
            }
            else
            {
                controlPoints = CubicFromTangents(p1, p2, Sides[2].Tangent0, Sides[2].Tangent1);
                CubicPoles.B210 = controlPoints[0];
                CubicPoles.B120 = controlPoints[1];
            }

            // Get middle pole position that gives us desired patch mid-point
            CubicPoles.B111 = GetMidPoleAlice();
        }

        /// <summary>Get inner control points of a Bezier cubic </summary>
        /// <param name="p1"> the first position of the curve </param>
        /// <param name="p2"> the second position of the curve </param>
        /// <param name="n1"> the first normal of the curve (unit vector)</param>
        /// <param name="n2"> the second normal of the curve (unit vector)</param>
        /// <returns> The two inner control points </returns>
        protected override Position[] CubicFromPtsNormals(Position p0, Position p1, Vector n0, Vector n1)
        {
            // Chord vector, and its length
            Vector c = p1 - p0;
            double k = Vector.Norm(c);
            Vector w = Vector.Unit(c);

            // Tangent directions are projections of the chord onto the tangent planes.
            // Therefore each tangent lies in plane of chord and corresponding normal
            Vector v0 = c - (c * n0) * n0;
            Vector u0 = Vector.Unit(v0);
            Vector v1 = c - (c * n1) * n1;
            Vector u1 = Vector.Unit(v1);

            // Length of Bezier polygon leg -- d0 = 2k / (3*(1 + cos alpha1))
            double cosAlpha0 = w * u0;
            double cosAlpha1 = w * u1;
            double d0 = (2 * k) / (3 * (1 + cosAlpha1));
            double d1 = (2 * k) / (3 * (1 + cosAlpha0));

            // Bezier control points
            Position pa = p0 + d0 * u0;
            Position pb = p1 - d1 * u1;
            Position[] poles = new Snap.Position[2] { pa, pb };
            return poles;
        }

       /// <summary>
       /// Calculate middle pole of a cubic Bezier triangle using the "Alice" technique
       /// </summary>
       /// <returns>
       /// Position of the middle pole
       /// Get the #10 pole of the cubic curved triangle
       /// </returns>
        protected override Position GetMidPoleAlice()
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
        private Position GetAuxPoint(Position p0, Position p1, Vector n0)
        {
            Vector c = p1 - p0;
            double k = Vector.Norm(c);
            Vector w = Vector.Unit(c);

            Vector v0 = c - (c*n0)*n0;
            Vector u0 = Vector.Unit(v0);
            double cosAlpha = w * u0;
            double d0 = (2 * k) / (3 * (1+cosAlpha));

            // Calculate Bezier points
            Position pa = p0 + d0 * u0;
            Position pb  = pa +  (1 - cosAlpha / 3) * c / (1 + cosAlpha);


            // Point at parameter value t = 1/3
            Position auxPoint = (p0 + 8 * p1 + 6 * pa + 12 * pb) / 27;

            return auxPoint;
        }
    }
}
