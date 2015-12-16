using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
    class ConeCubicTriangle : CubicCurvedTriangle
    {
        /// <summary> the center of the base plane of the cone </summary>
        private Position basePoint;
        /// <summary> the tip of the cone </summary>
        private Position topPoint;
        /// <summary> the half angle of the cone </summary>
        private double halfAngle;
        /// <summary> radius of the circle on the base plane </summary>
        private double radius;

        /// <summary> construct a cone </summary>
        public ConeCubicTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID, 
            NXOpen.Facet.FacetedFace.FacetedfaceType faceType, NXOpen.Facet.FacetedFace face)
            :base(vertexs, sides, facetID, faceID, faceType)
        {
            NXOpen.Point3d position = new NXOpen.Point3d();
            NXOpen.Point3d direction = new NXOpen.Point3d();
            bool sense;
            //##################################################################
            //##################################################################
            // Please notice our cone cubic curved triangle doesn't work well
            // GetSurfaceData always cause problem: Memory access violation
            // We haven't fixed this bug yet
            //##################################################################
            //##################################################################
            try
            {
                face.GetSurfaceData(out position, out direction, out radius, out halfAngle, out sense);
            }
            catch (Exception)
            {
                face.Color = 1;
                throw;
            }
            //##################################################################
            //##################################################################
            //##################################################################
            //##################################################################

            basePoint = position;
            Vector axisDirection = new Vector(direction.X, direction.Y, direction.Z);
            topPoint = basePoint + (radius / System.Math.Tan(halfAngle)) * Vector.Unit(axisDirection);

            cubicPoles = new CubicTrianglePoles();
            GetCubicTrianglePoles();
        }

        /// <summary> Get the ten control points of the bezier patch </summary>
        /// <param name="triangle"> three vertexs </param>
        /// <returns> Ten control points </returns>
        protected override void GetCubicTrianglePoles()
        {
            Position p1 = vertexs[0].Point;
            Position p2 = vertexs[1].Point;
            Position p3 = vertexs[2].Point;

            Vector n1 = vertexs[0].Normal;
            Vector n2 = vertexs[1].Normal;
            Vector n3 = vertexs[2].Normal;


            Position tip = topPoint;
            Vector ea = basePoint - topPoint;

            CubicPoles.B300 = p1;
            CubicPoles.B030 = p2;
            CubicPoles.B003 = p3;

            Position[] controlPoints = new Position[2];

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
        protected override Position[] CubicFromPtsNormals(Position p1, Position p2, Vector n1, Vector n2)
        {
            Position[] poles = new Position[2];
            if (System.Math.Abs(Vector.Unit(p1 - p2) * n1) < 1e-4 || System.Math.Abs(Vector.Unit(p1 - p2) * n2) < 1e-4)
            {
                poles[0] = (2 * p1 + p2) / 3;
                poles[1] = (p1 + 2 * p2) / 3;
            }
            else
            {
                Vector ea = basePoint - topPoint;
                Position tip = topPoint;

                // The Apex Point
                Position pa;

                // Get the shoulder point of the conic section, Details refer to section 16.9-10 in George's book
                // We make the shoulder point lie on the cone
                // Pa(the Apex Point) is where the two rays intersect, rho parameter can adjust the shape of the curve
                // rho = PsPm/PaPm (Pm is the mid point of chord P0P1, Ps is the shoulder point)
                double rho  = GetRhoValue(p1, p2, n1, n2, out pa);


                double d0 = 4 * rho * Vector.Norm(pa - p1) / 3;
                double d1 = 4 * rho * Vector.Norm(pa - p2) / 3;

                poles[0] = p1 + d0 * Vector.Unit(pa - p1);
                poles[1] = p2 + d1 * Vector.Unit(pa - p2);
            }

            return poles;

        }

        protected override Position GetMidPoleAlice()
        {
            Position mid12P = (CubicPoles.B300 + CubicPoles.B030 + 3 * CubicPoles.B210 + 3 * CubicPoles.B120) / 8;
            Position mid23P = (CubicPoles.B030 + CubicPoles.B003 + 3 * CubicPoles.B021 + 3 * CubicPoles.B012) / 8;
            Position mid13P = (CubicPoles.B300 + CubicPoles.B003 + 3 * CubicPoles.B102 + 3 * CubicPoles.B201) / 8;

            Vector tagentMid12 = Vector.Cross(mid12P - topPoint, basePoint - topPoint);
            Vector mid12N = Vector.UnitCross(mid12P - topPoint, tagentMid12);

            Vector tagentMid23 = Vector.Cross(mid23P - topPoint, basePoint - topPoint);
            Vector mid23N = Vector.UnitCross(mid23P - topPoint, tagentMid23);

            Vector tagentMid13 = Vector.Cross(mid13P - topPoint, basePoint - topPoint);
            Vector mid13N = Vector.UnitCross(mid13P - topPoint, tagentMid13);

            Position auxPoint1 = GetAuxPoint(CubicPoles.B300, mid23P, vertexs[0].Normal, mid23N);
            Position auxPoint2 = GetAuxPoint(CubicPoles.B030, mid13P, vertexs[1].Normal, mid13N);
            Position auxPoint3 = GetAuxPoint(CubicPoles.B003, mid12P, vertexs[2].Normal, mid12N);

            Position auxPole = (auxPoint1 + auxPoint2 + auxPoint3) / 3;
            Position midPole = (Position)(27 * auxPole - (CubicPoles.B300 + CubicPoles.B030 + CubicPoles.B003 + 3 * CubicPoles.B210 + 3 * CubicPoles.B120 + 3 * CubicPoles.B021 + 3 * CubicPoles.B012 + 3 * CubicPoles.B102 + 3 * CubicPoles.B201)) / 6;
            return midPole;
        }

        /// <summary> Get the auxiliary point (for the mid pole of the triangle) </summary>
        /// <param name="p1"> The first position of the curve </param>
        /// <param name="p2"> The second position of the curve </param>
        /// <param name="n1"> The first normal of the curve </param>
        /// <param name="n2"> The second normal of the curve </param>
        /// <returns>The one-third point (t=1/3) on the arc-like curve</returns>
        private Position GetAuxPoint(Position p1, Position p2, Vector n1, Vector n2)
        {
            Position[] controlPoints = CubicFromPtsNormals(p1, p2, n1, n2);

            // Point at parameter value t = 1/3
            Position auxPoint = (p1 + 8 * p2 + 6 * controlPoints[0] + 12 * controlPoints[1]) / 27;
            return auxPoint;
        }

        /// <summary> Get the shoulder point of the curve </summary>
        /// <param name="p0"> the first position of the curve </param>
        /// <param name="p1"> the second position of the curve </param>
        /// <param name="n0"> the first normal of the curve </param>
        /// <param name="n1"> the second normal of the curve </param>
        /// <param name="pa"> the apex point of the control polygon </param>
        /// <returns> the rho value of the curve </returns>
        private double GetRhoValue(Position p0, Position p1, Vector n0, Vector n1, out Position pa)
        {
            double rho;
            if (System.Math.Abs(Vector.Unit(p0 - p1) * n0) < 1e-4 || System.Math.Abs(Vector.Unit(p0 - p1) * n1) < 1e-4)
            {
                pa = (p0 + p1) / 2;
                rho = 0;
                return rho;
            }
            Vector axisVector = basePoint - topPoint;
            Position tip = topPoint;

            Vector tangent0 = Vector.UnitCross(n0, axisVector);
            if (tangent0 * (p1 - p0) < 0)
            {
                tangent0 = -tangent0;
            }
            Vector tangent1 = Vector.UnitCross(n1, axisVector);
            if (tangent1 * (p0 - p1) < 0)
            {
                tangent1 = -tangent1;
            }

            Compute.DistanceResult result = Snap.Compute.ClosestPoints(new Snap.Geom.Curve.Ray(p0, tangent0), new Snap.Geom.Curve.Ray(p1, tangent1));
            pa = ((result.Point1 + result.Point2) / 2);

            Position pm = (p0 + p1) / 2;
            rho = MathFunctions.solveQuadraticEqt(pa - pm, pm - tip, axisVector, System.Math.Cos(halfAngle));
            return rho;
        }
    }
}
