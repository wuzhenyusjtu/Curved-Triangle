using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace TestsOnModels
{
    class QuadCurvedTriangle : Triangle
    {
        protected QuadTrianglePoles quadPoles;
        /// <summary> Six control points of the quadratic triangle </summary>
        public QuadTrianglePoles QuadPoles { get { return quadPoles; } set { quadPoles = value; } }


        public QuadCurvedTriangle(Vertex[] vertexs)
            : base(vertexs)
        {
            quadPoles = new QuadTrianglePoles();
            GetSixPolesWithWeights();
        }

        private void GetSixPolesWithWeights()
        {
            Position p1 = vertexs[0].Point;
            Position p2 = vertexs[1].Point;
            Position p3 = vertexs[2].Point;

            Vector n1 = vertexs[0].Normal;
            Vector n2 = vertexs[1].Normal;
            Vector n3 = vertexs[2].Normal;

            QuadPoles.P00 = p1;
            QuadPoles.P02 = p2;
            QuadPoles.P20 = p3;

            QuadPoles.W00 = 1;
            QuadPoles.W02 = 1;
            QuadPoles.W20 = 1;

            Position outValuePosition;
            double outValueWeight;
            QuadraticFromPtsNormals(p2, p3, n2, n3, out outValuePosition, out outValueWeight);
            QuadPoles.P11 = outValuePosition;
            QuadPoles.W11 = outValueWeight;
            QuadraticFromPtsNormals(p3, p1, n3, n1, out outValuePosition, out outValueWeight);
            QuadPoles.P10 = outValuePosition;
            QuadPoles.W10 = outValueWeight;
            QuadraticFromPtsNormals(p1, p2, n1, n2, out outValuePosition, out outValueWeight);
            QuadPoles.P01 = outValuePosition;
            QuadPoles.W01 = outValueWeight;
        }

        private void QuadraticFromPtsNormals(Snap.Position p1, Snap.Position p2, Snap.Vector n1, Snap.Vector n2, out Snap.Position p12, out double weight)
        {
            // Apex point between q0 and q1
            Snap.Vector c = p2 - p1;
            Snap.Vector b = Snap.Vector.Cross(n1, c);
            Snap.Vector h = Snap.Vector.UnitCross(b, n1);
            c = p1 - p2;
            b = Snap.Vector.Cross(n2, c);
            Snap.Vector k = Snap.Vector.UnitCross(b, n2);
            if (Snap.Vector.Angle(h, k) > 179)
            {
                p12 = (p1 + p2) / 2;
                weight = 1;
            }
            else
            {
                Snap.Compute.DistanceResult result = Snap.Compute.ClosestPoints(new Snap.Geom.Curve.Ray(p1, h), new Snap.Geom.Curve.Ray(p2, k));
                p12 = (result.Point1 + result.Point2) / 2;
                weight = System.Math.Sqrt((1 - h * k) / 2);
            }
        }

        #region Methods to cope with Infelxion Points, Build Cubic Triangle instead of Quadratic Triangle
        public bool AnyInflexionPt()
        {
            Snap.Position p0 = (Snap.Position)vertexs[0].Point;
            Snap.Position p1 = (Snap.Position)vertexs[1].Point;
            Snap.Position p2 = (Snap.Position)vertexs[2].Point;
            Snap.Vector n0 = vertexs[0].Normal;
            Snap.Vector n1 = vertexs[1].Normal;
            Snap.Vector n2 = vertexs[2].Normal;

            Snap.Vector c01 = p1 - p0;
            Snap.Vector b01 = Snap.Vector.Cross(n0, c01);
            Snap.Vector tangent01 = Snap.Vector.UnitCross(b01, n0);
            Snap.Vector c10 = p0 - p1;
            Snap.Vector b10 = Snap.Vector.Cross(n1, c10);
            Snap.Vector tangent10 = Snap.Vector.UnitCross(b10, n1);

            Snap.Vector c12 = p2 - p1;
            Snap.Vector b12 = Snap.Vector.Cross(n1, c12);
            Snap.Vector tangent12 = Snap.Vector.UnitCross(b12, n1);
            Snap.Vector c21 = p1 - p2;
            Snap.Vector b21 = Snap.Vector.Cross(n2, c21);
            Snap.Vector tangent21 = Snap.Vector.UnitCross(b21, n2);

            Snap.Vector c20 = p2 - p0;
            Snap.Vector b20 = Snap.Vector.Cross(n2, c20);
            Snap.Vector tangent20 = Snap.Vector.UnitCross(b20, n2);
            Snap.Vector c02 = p0 - p2;
            Snap.Vector b02 = Snap.Vector.Cross(n0, c02);
            Snap.Vector tangent02 = Snap.Vector.UnitCross(b02, n0);

            Snap.Vector e0 = p1 - p0;
            Snap.Vector e1 = p2 - p1;
            Snap.Vector e2 = p0 - p2;

            if (Snap.Vector.Cross(e0, tangent01) * Snap.Vector.Cross(tangent10, e0) > 0)
            {
                //Snap.NX.Line line = Snap.Create.Line(p0, p1);
                return true;
            }
            if (Snap.Vector.Cross(e1, tangent12) * Snap.Vector.Cross(tangent21, e1) > 0)
            {
                //Snap.NX.Line line = Snap.Create.Line(p1, p2);
                return true;
            }
            if (Snap.Vector.Cross(e2, tangent20) * Snap.Vector.Cross(tangent02, e2) > 0)
            {
                //Snap.NX.Line line = Snap.Create.Line(p0, p2);
                return true;
            }
            return false;
        }

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

        private Snap.Position[] CubicFromPtsNormals(Snap.Position p0, Snap.Position p1, Snap.Vector n0, Snap.Vector n1)
        {
            // Chord Snap.Vector, and its length
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
            Snap.Position[] poles = { pa, pb };
            return poles;
        }

        private Snap.Position QuadraticFromPtsNormals(Snap.Position p0, Snap.Position p1, Snap.Vector n0, Snap.Vector n1)
        {
            // Apex point between p0 and p1
            Snap.Vector c = p1 - p0;
            Snap.Vector b = Snap.Vector.Cross(n0, c);
            Snap.Vector h = Snap.Vector.UnitCross(b, n0);
            c = p0 - p1;
            b = Snap.Vector.Cross(n1, c);
            Snap.Vector k = Snap.Vector.UnitCross(b, n1);
            if (Snap.Vector.Angle(h, k) > 179)
            {
                Snap.Position p01 = new Snap.Position();
                return p01 = (p0 + p1) / 2;
            }
            else
            {
                Snap.Compute.DistanceResult result = Snap.Compute.ClosestPoints(new Snap.Geom.Curve.Ray(p0, h), new Snap.Geom.Curve.Ray(p1, k));
                Snap.Position p01 = new Snap.Position();
                return p01 = (result.Point1 + result.Point2) / 2;
            }
        }

        private Snap.Position[] DegreeElevation(Snap.Position[] quadraticPoles)
        {
            Snap.Position b0 = quadraticPoles[0];
            Snap.Position b1 = quadraticPoles[1];
            Snap.Position b2 = quadraticPoles[2];

            Snap.Position c1 = b0 / 3 + 2 * b1 / 3;
            Snap.Position c2 = 2 * b1 / 3 + b2 / 3;

            Snap.Position[] cubicPoles = new Snap.Position[2] { c1, c2 };
            return cubicPoles;
        }

        /// <summary>
        /// Get the mid(#10) pole of the cubic curved triangle
        /// </summary>
        /// <param name="marginPoles">The poles on the sides of the cubic curved triangle</param>
        /// <param name="vertexs"></param>
        /// <returns>The mid pole</returns>
        private Snap.Position GetMidPoleAlice(CubicTrianglePoles CubicPoles)
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
        
        #endregion

    }

}
