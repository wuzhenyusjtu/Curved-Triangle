using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
    /// <summary>Class representing a quadratic curved triangle (three vertexs and ten bezier control points)</summary>
    abstract class QuadCurvedTriangle : Triangle
    {
        protected QuadTrianglePoles quadPoles;
        /// <summary> Six control points of the quadratic triangle </summary>
        public QuadTrianglePoles QuadPoles { get { return quadPoles; } set { quadPoles = value; } }


        public QuadCurvedTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID,
            NXOpen.Facet.FacetedFace.FacetedfaceType faceType, QuadTrianglePoles quadPoles)
            :base(vertexs, sides, facetID, faceID,faceType)
        {
            this.quadPoles = quadPoles;
        }

        public QuadCurvedTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID, 
            NXOpen.Facet.FacetedFace.FacetedfaceType faceType)
            : base(vertexs, sides, facetID, faceID, faceType)
        {
        }

       // Gets implemented in special case code for sphere, cylinder, cone
        abstract protected void GetSixPolesWithWeights();

        abstract protected void QuadraticFromPtsNormals(Position p1, Position p2, Vector n1, Vector n2, out Position p12, out double weight);

        #region Methods to cope with Infelxion Points, Build Cubic Triangle instead of Quadratic Triangle

        /// <summary>
        /// Quadratic curve cannot deal with inflexion pt, producing very bad interpolation 
        /// Check if the cubic curve derived from the points and normals has inflexion point, 
        /// if it does, we will construct a cubic triangle instead of a quadratic one
        /// </summary>
        /// <returns>Bool value indicator</returns>
        public bool AnyInflexionPt()
        {
            Position p0 = vertexs[0].Point;
            Position p1 = vertexs[1].Point;
            Position p2 = vertexs[2].Point;
            Vector n0 = vertexs[0].Normal;
            Vector n1 = vertexs[1].Normal;
            Vector n2 = vertexs[2].Normal;

            Vector c01 = p1 - p0;
            Vector b01 = Snap.Vector.Cross(n0, c01);
            Vector tangent01 = Snap.Vector.UnitCross(b01, n0);
            Vector c10 = p0 - p1;
            Vector b10 = Snap.Vector.Cross(n1, c10);
            Vector tangent10 = Snap.Vector.UnitCross(b10, n1);

            Vector c12 = p2 - p1;
            Vector b12 = Snap.Vector.Cross(n1, c12);
            Vector tangent12 = Snap.Vector.UnitCross(b12, n1);
            Vector c21 = p1 - p2;
            Vector b21 = Snap.Vector.Cross(n2, c21);
            Vector tangent21 = Snap.Vector.UnitCross(b21, n2);

            Vector c20 = p2 - p0;
            Vector b20 = Snap.Vector.Cross(n2, c20);
            Vector tangent20 = Snap.Vector.UnitCross(b20, n2);
            Vector c02 = p0 - p2;
            Vector b02 = Snap.Vector.Cross(n0, c02);
            Vector tangent02 = Snap.Vector.UnitCross(b02, n0);

            Vector e0 = p1 - p0;
            Vector e1 = p2 - p1;
            Vector e2 = p0 - p2;

            if (Vector.Cross(e0, tangent01) * Vector.Cross(tangent10, e0) > 0)
            {
                return true;
            }
            if (Vector.Cross(e1, tangent12) * Vector.Cross(tangent21, e1) > 0)
            {
                return true;
            }
            if (Vector.Cross(e2, tangent20) * Vector.Cross(tangent02, e2) > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// If inflexion pts exist, we will construct cubic triangle instead
        /// Triangle has three sides
        /// If no inflexion pt on a side, construct quadratic, then elevate the degree to cubic
        /// </summary>
        /// <returns>Ten control points for a cubic curved triangle</returns>
        public CubicTrianglePoles ConvertToCubic()
        {
            Position p0 = vertexs[0].Point;
            Position p1 = vertexs[1].Point;
            Position p2 = vertexs[2].Point;

            Vector n0 = vertexs[0].Normal;
            Vector n1 = vertexs[1].Normal;
            Vector n2 = vertexs[2].Normal;

            Vector c01 = p1 - p0;
            Vector b01 = Snap.Vector.Cross(n0, c01);
            Vector tangent01 = Snap.Vector.UnitCross(b01, n0);
            Vector c10 = p0 - p1;
            Vector b10 = Snap.Vector.Cross(n1, c10);
            Vector tangent10 = Snap.Vector.UnitCross(b10, n1);

            Vector c12 = p2 - p1;
            Vector b12 = Snap.Vector.Cross(n1, c12);
            Vector tangent12 = Snap.Vector.UnitCross(b12, n1);
            Vector c21 = p1 - p2;
            Vector b21 = Snap.Vector.Cross(n2, c21);
            Vector tangent21 = Snap.Vector.UnitCross(b21, n2);

            Vector c20 = p2 - p0;
            Vector b20 = Snap.Vector.Cross(n2, c20);
            Vector tangent20 = Snap.Vector.UnitCross(b20, n2);
            Vector c02 = p0 - p2;
            Vector b02 = Snap.Vector.Cross(n0, c02);
            Vector tangent02 = Snap.Vector.UnitCross(b02, n0);

            Vector e0 = p1 - p0;
            Vector e1 = p2 - p1;
            Vector e2 = p0 - p2;

            Position[] cubicPolesOnE1 = new Position[2];
            Position[] cubicPolesOnE2 = new Position[2];
            Position[] cubicPolesOnE0 = new Position[2];

            if (Vector.Cross(e0, tangent01) * Vector.Cross(tangent10, e0) > 0)
            {
                cubicPolesOnE1 = CubicFromPtsNormals(p0, p1, n0, n1);
            }
            else
            {
                Position quadraticPoleOnE1 = QuadraticFromPtsNormals(p0, p1, n0, n1);
                cubicPolesOnE1 = DegreeElevation(new Position[3] { p0, quadraticPoleOnE1, p1 });
            }

            if (Vector.Cross(e1, tangent12) * Vector.Cross(tangent21, e1) > 0)
            {
                cubicPolesOnE2 = CubicFromPtsNormals(p1, p2, n1, n2);
            }
            else
            {
                Position quadraticPoleOnE2 = QuadraticFromPtsNormals(p1, p2, n1, n2);
                cubicPolesOnE2 = DegreeElevation(new Position[3] { p1, quadraticPoleOnE2, p2 });
            }

            if (Vector.Cross(e2, tangent20) * Vector.Cross(tangent02, e2) > 0)
            {
                cubicPolesOnE0 = CubicFromPtsNormals(p2, p0, n2, n0);
            }
            else
            {
                Position quadraticPoleOnE0 = QuadraticFromPtsNormals(p2, p0, n2, n0);
                cubicPolesOnE0 = DegreeElevation(new Position[3] { p2, quadraticPoleOnE0, p0 });
            }
            CubicTrianglePoles cubicPoles = new CubicTrianglePoles();
            cubicPoles.B300 = p0;
            cubicPoles.B030 = p1;
            cubicPoles.B003 = p2;
            cubicPoles.B210 = cubicPolesOnE1[0];
            cubicPoles.B120 = cubicPolesOnE1[1];
            cubicPoles.B021 = cubicPolesOnE2[0];
            cubicPoles.B012 = cubicPolesOnE2[1];
            cubicPoles.B102 = cubicPolesOnE0[0];
            cubicPoles.B201 = cubicPolesOnE0[1];
            // Get middle pole position that gives us desired patch mid-point
            cubicPoles.B111 = GetMidPoleAlice(cubicPoles);
            return cubicPoles;
        }

        /// <summary>
        /// Calculate middle pole of a cubic Bezier triangle using the "Alice" technique
        /// </summary>
        /// <param name="marginPoles">The nine poles around the edge of the patch</param>
        /// <param name="vertexs">The vertices of the riangular facet</param>
        /// <returns>Position of the middle pole</returns>
        protected Position GetMidPoleAlice(CubicTrianglePoles CubicPoles)
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

            Vector v0 = c - (c * n0) * n0;
            Vector u0 = Vector.Unit(v0);
            double cosAlpha = w * u0;
            double d0 = (2 * k) / (3 * (1 + cosAlpha));

            // Calculate Bezier points
            Position pa = p0 + d0 * u0;
            Position pb = pa + (1 - cosAlpha / 3) * c / (1 + cosAlpha);


            // Point at parameter value t = 1/3
            Position auxPoint = (p0 + 8 * p1 + 6 * pa + 12 * pb) / 27;

            return auxPoint;
        }
        
        /// <summary>
        /// Get inner control points of a Bezier cubic curve
        /// </summary>
        /// <param name="p0">The start point of the curve </param>
        /// <param name="p1">The end point of the curve </param>
        /// <param name="n0">Normal at the start point (unit vector)</param>
        /// <param name="n1">Normal at the end point (unit vector)</param>
        /// <returns>The two inner control points, pa and pb </returns>
        private Position[] CubicFromPtsNormals(Position p0, Position p1, Vector n0, Vector n1)
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
            Position[] poles = { pa, pb };
            return poles;
        }

        /// <summary>
        /// Get inner control point of a Bezier quadratic curve, the weight of the pole is 1
        /// </summary>
        /// <param name="p0">The start point of the curve </param>
        /// <param name="p1">The end point of the curve </param>
        /// <param name="n0">Normal at the start point (unit vector)</param>
        /// <param name="n1">Normal at the end point (unit vector)</param>
        /// <returns>The inner control point of quadratic curve, the weight of the point is 1 </returns>
        private Position QuadraticFromPtsNormals(Position p0, Position p1, Vector n0, Vector n1)
        {
            // Apex point between p0 and p1
            Vector c = p1 - p0;
            Vector b = Vector.Cross(n0, c);
            Vector h = Vector.UnitCross(b, n0);
            c = p0 - p1;
            b = Vector.Cross(n1, c);
            Vector k = Vector.UnitCross(b, n1);
            if (Vector.Angle(h, k) > 179)
            {
                Position pa = new Position();
                return pa = (p0 + p1) / 2;
            }
            else
            {
                // Pa is the intersection point of two rays 
                Compute.DistanceResult result = Snap.Compute.ClosestPoints(new Snap.Geom.Curve.Ray(p0, h), new Snap.Geom.Curve.Ray(p1, k));
                Position pa = new Position();
                return pa = (result.Point1 + result.Point2) / 2;
            }
        }

        /// <summary>
        /// Degree elevation from quadratic to cubic
        /// </summary>
        /// <param name="quadraticPoles">3 control points for a quadratic Bezier curve</param>
        /// <returns>2 inner control points for a cubic Bezier curve, Pa & Pb</returns>
        private Position[] DegreeElevation(Position[] quadraticPoles)
        {
            Position b0 = quadraticPoles[0];
            Position b1 = quadraticPoles[1];
            Position b2 = quadraticPoles[2];

            Position c1 = b0 / 3 + 2 * b1 / 3;
            Position c2 = 2 * b1 / 3 + b2 / 3;

            Position[] cubicPoles = new Position[2] { c1, c2 };
            return cubicPoles;
        }

        #endregion

        /// <summary>
        /// Get inner control point and associated weight for a quadratic Bezier curve
        /// The point and weight are directly derived from tangents rather than points and normals
        /// </summary>
        /// <param name="p0">The start point of the curve </param>
        /// <param name="p1">The end point of the curve </param>
        /// <param name="n0">Normal at the start point (unit vector)</param>
        /// <param name="n1">Normal at the end point (unit vector)</param>
        /// <param name="pa">The control point</param>
        /// <param name="weight">Shape parameter, If we increase it, the curve is pulled towards the pole</param>
        protected void QuadraticFromTangents(Position p0, Position p1, Vector t0, Vector t1, out Position pa, out double weight)
        {
            if (Vector.Angle(t0, t1) > 179)
            {
                pa = (p0 + p1) / 2;
                weight = 1;
            }
            else
            {
                Compute.DistanceResult result = Snap.Compute.ClosestPoints(new Snap.Geom.Curve.Ray(p0, t0), new Snap.Geom.Curve.Ray(p1, t1));
                pa = (result.Point1 + result.Point2) / 2;
                weight = System.Math.Sqrt((1 - t0 * t1) / 2);   // George's formula
            }
        }
  }
}
