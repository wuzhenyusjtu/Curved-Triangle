using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
   /// <summary>
   /// Construct cubic Bezier triangular patch that lies on a circular cylinder
   /// </summary>
    class CylCubicTriangle : CubicCurvedTriangle
    {

        public CylCubicTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID, NXOpen.Facet.FacetedFace.FacetedfaceType faceType)
            :base(vertexs, sides, facetID, faceID,faceType)
        {
            cubicPoles = new CubicTrianglePoles();
            GetCubicTrianglePoles();
        }

        /// <summary>Get the ten control points of the bezier cubic triangular patch lying on a cylinder</summary>
        /// <returns> ten control points </returns>
        protected override void GetCubicTrianglePoles()
        {
            Position[] cubicTrianglePoles = new Position[10];

            Position p1 = vertexs[0].Point;
            Position p2 = vertexs[1].Point;
            Position p3 = vertexs[2].Point;

            Vector n1 = vertexs[0].Normal;
            Vector n2 = vertexs[1].Normal;
            Vector n3 = vertexs[2].Normal;

            Position[] controlPoints = new Position[2];

            CubicPoles.B300 = p1;
            CubicPoles.B030 = p2;
            CubicPoles.B003 = p3;

            // six control points to create circles-like cubic edges

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

        /// <summary>Get inner control points of a Bezier cubic (lying on a cylinder)</summary>
        /// <param name="p0"> the first position of the curve </param>
        /// <param name="p1"> the second position of the curve </param>
        /// <param name="n0"> the first normal of the curve (unit vector)</param>
        /// <param name="n1"> the second normal of the curve (unit vector)</param>
        /// <returns> The two inner control points </returns>
        protected override Position[] CubicFromPtsNormals(Position p0, Position p1, Vector n0, Vector n1)
        {
            Position[] poles = new Position[2];

            // Case where normals are not parallel
            if (Vector.Norm(Vector.Cross(n1, n0)) > 0.01)
            {
                // Axis vector of cylinder 
                Vector e = Vector.UnitCross(n1, n0);

                // p1 prime is projected along cylinder, so it lies on same circle as p2
                Position p0Prime = p0 + ((p1 - p0) * e) * e;
                Vector n0Prime = n0;

                // Chord between p1prime and p2 
                Vector p01Prime = p1 - p0Prime;
                double k = Vector.Norm(p01Prime);

                // Get cosAlpha from sine of complement (cross product gives sine) 
                double cosAlpha0 = Vector.Norm(Vector.Cross(Vector.Unit(p01Prime), Vector.Unit(n0Prime)));

                double cosAlpha1 = Vector.Norm(Vector.Cross(Vector.Unit(-p01Prime), Vector.Unit(n1)));

                // Length of Bezier polygon leg, refer to the formula d = k / 2 (1 + cosAlpha), in section 17.8 of George's book, 
                double d0 = (2 * k) / (3 * (1 + cosAlpha1));
                double d1 = (2 * k) / (3 * (1 + cosAlpha0));

                // We assume that the curve lies in a plane containing p1 and p2, and perpendicular to (n0 + n1)/2
                // So, the normal of this plane is ...
                Vector nEllipse= Vector.UnitCross((n0 + n1) / 2, p1 - p0); 

                // Tangent vector is perpendicular to n1 and np1p2
                Vector tangent0 = Vector.UnitCross(nEllipse, n0);

                // But we need to correct the direction of the tangent
                if (tangent0 * (p1 - p0) < 0)
                {
                    tangent0 = -tangent0;
                }

                // The following formula gets the control point on the circle, and then projects
                // it back to the plane of the ellipse
                Position pa = p0 + (d0 / (Vector.Norm(Vector.Cross(tangent0, e)))) * tangent0;

                Vector tangent1 = Vector.UnitCross(nEllipse, n1);
                if (tangent1 * (p0 - p1) < 0)
                {
                    tangent1 = -tangent1;
                }

                Position pb = p1 + (d1 / (Vector.Norm(Vector.Cross(tangent1, e)))) * tangent1;

                poles[0] = pa;
                poles[1] = pb;
            }

            // case where normals are (nearly) parallel -- just make a straight line cubic
            else
            {
                poles[0] = 2 * p0 / 3 + p1 / 3;
                poles[1] = 2 * p1 / 3 + p0 / 3;
            }

            return poles;
        }

        /// <summary>
        /// Get the #10 pole of the cubic curved triangle
        /// </summary>
        /// <returns></returns>
        protected override Position GetMidPoleAlice()
        {
            // Calculate mid-points of edges (t = 1/2)
            Position mid12 = (CubicPoles.B300 + CubicPoles.B030 + 3 * CubicPoles.B210 + 3 * CubicPoles.B120) / 8;
            Position mid23 = (CubicPoles.B030 + CubicPoles.B003 + 3 * CubicPoles.B021 + 3 * CubicPoles.B012) / 8;
            Position mid13 = (CubicPoles.B300 + CubicPoles.B003 + 3 * CubicPoles.B102 + 3 * CubicPoles.B201) / 8;

            // We assume that the normal vector at mid23 is the average of normals at p2 and p3.
            // Doesn't seem quite right??
            Position c1 = GetAuxPoint(CubicPoles.B300, mid23, vertexs[0].Normal, (vertexs[1].Normal + vertexs[2].Normal) / 2);
            Position c2 = GetAuxPoint(CubicPoles.B030, mid13, vertexs[1].Normal, (vertexs[0].Normal + vertexs[2].Normal) / 2);
            Position c3 = GetAuxPoint(CubicPoles.B003, mid12, vertexs[2].Normal, (vertexs[0].Normal + vertexs[1].Normal) / 2);

            // If c1, c2, c3 lie on cylinder, then their average will not lie on cylinder (exactly)
            // Can we find a suitable c that lies exactly on the cylinder ??
            Position auxPole = (c1 + c2 + c3) / 3;
            Position midPole = (Position)(27 * auxPole - (CubicPoles.B300 + CubicPoles.B030 + CubicPoles.B003 + 3 * CubicPoles.B210 + 3 * CubicPoles.B120 + 3 * CubicPoles.B021 + 3 * CubicPoles.B012 + 3 * CubicPoles.B102 + 3 * CubicPoles.B201)) / 6;
            //Snap.InfoWindow.WriteLine("(" + midPole.X.ToString() + "," + midPole.Y.ToString() + "," + midPole.Z.ToString() + ")");
            return midPole;
        }

        /// <summary>Get an auxiliary point (t=1/3) on a cubic (for mid pole of the triangle) </summary>
        /// <param name="p1"> the first position of the curve </param>
        /// <param name="p2"> the second position of the curve </param>
        /// <param name="n1"> the first normal of the curve </param>
        /// <param name="n2"> the second normal of the curve </param>
        /// <returns> the t = 1/3 point </returns>
        private Position GetAuxPoint(Position p1, Position p2, Vector n1, Vector n2)
        {
            // Get control points
            Position[] controlPoints = CubicFromPtsNormals(p1, p2, n1, n2);

            // Calculate point at t = 1/3
            Position auxPoint = (p1 + 8 * p2 + 6 * controlPoints[0] + 12 * controlPoints[1]) / 27;

            return auxPoint;
        }
    }
}
