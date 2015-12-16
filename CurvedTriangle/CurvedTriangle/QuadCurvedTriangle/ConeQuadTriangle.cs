using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
    class ConeQuadTriangle : QuadCurvedTriangle
    {
        /// <summary> the center of the base plane of the cone </summary>
        private Position basePoint;
        /// <summary> the tip of the cone </summary>
        private Position topPoint;
        /// <summary> the half angle of the cone </summary>
        private double halfAngle;
        /// <summary> radius of the circle on the base plane </summary>
        private double radius;

        public ConeQuadTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID,
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

            quadPoles = new QuadTrianglePoles();
            GetSixPolesWithWeights();
        }

       /// <summary>
       /// Computes poles and weights for a rational quadratic triangular Bezier patch
       /// </summary>
       /// <param name="sixPoles"></param>
       /// <param name="sixWeights"></param>
        protected override void GetSixPolesWithWeights()
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
            if (Sides[0].IsOnEdge == false)
            {
                QuadraticFromPtsNormals(p2, p3, n2, n3, out outValuePosition, out outValueWeight);
                QuadPoles.P11 = outValuePosition;
                QuadPoles.W11 = outValueWeight;
            }
            else
            {
                QuadraticFromTangents(p2, p3, Sides[0].Tangent0, Sides[0].Tangent1, out outValuePosition, out outValueWeight);
                QuadPoles.P11 = outValuePosition;
                QuadPoles.W11 = outValueWeight;
            }
            if (Sides[1].IsOnEdge == false)
            {
                QuadraticFromPtsNormals(p3, p1, n3, n1, out outValuePosition, out outValueWeight);
                QuadPoles.P10 = outValuePosition;
                QuadPoles.W10 = outValueWeight;
            }
            else
            {
                QuadraticFromTangents(p3, p1, Sides[1].Tangent0, Sides[1].Tangent1, out outValuePosition, out outValueWeight);
                QuadPoles.P10 = outValuePosition;
                QuadPoles.W10 = outValueWeight;
            }
            if (Sides[2].IsOnEdge == false)
            {
                QuadraticFromPtsNormals(p1, p2, n1, n2, out outValuePosition, out outValueWeight);
                QuadPoles.P01 = outValuePosition;
                QuadPoles.W01 = outValueWeight;
            }
            else
            {
                QuadraticFromTangents(p1, p2, Sides[2].Tangent0, Sides[2].Tangent1, out outValuePosition, out outValueWeight);
                QuadPoles.P01 = outValuePosition;
                QuadPoles.W01 = outValueWeight;
            }
        }

        /// <summary> figure out the weight of the mid pole for cone </summary>
        /// <param name="p0"> the first position of the curve </param>
        /// <param name="p1"> the second position of the curve </param>
        /// <param name="n0"> the first normal of the curve </param>
        /// <param name="n1"> the second normal of the curve </param>
        /// <param name="pa"> the mid pole</param>
        /// <param name="weight"> the weight of the mid pole </param>
        protected override void QuadraticFromPtsNormals(Position p0, Position p1, Vector n0, Vector n1, out Position pa, out double weight)
        {
            if (System.Math.Abs(Vector.Unit(p0 - p1) * n0) < 1e-4 || System.Math.Abs(Vector.Unit(p0 - p1) * n1) < 1e-4)
            {
                pa = (p0 + p1) / 2;
                weight = 1;
                return;
            }

            Position tip = topPoint;
            Vector axisVector = basePoint - topPoint;

            Vector tagent1 = Vector.UnitCross(n0, axisVector);
            Vector tagent2 = Vector.UnitCross(n1, axisVector);

            Vector tangent0 = Vector.UnitCross(n0, axisVector);
            if (tangent0 * (p1 - p0) < 0)
            {
                tangent0 = -tangent0;
            }
            Vector tangent1 = Vector.UnitCross(n0, axisVector);
            if (tangent1 * (p0 - p1) < 0)
            {
                tangent1 = -tangent1;
            }


            if (Vector.Angle(tagent1, tagent2) > 179)
            {
                pa = (p0 + p1) / 2;
                weight = 1;
            }
            else
            {
                Compute.DistanceResult result = Snap.Compute.ClosestPoints(new Snap.Geom.Curve.Ray(p0, tagent1), new Snap.Geom.Curve.Ray(p1, tagent2));
                pa = (result.Point1 + result.Point2) / 2;
                Position pm = (p0 + p1) / 2;

                double rho = MathFunctions.solveQuadraticEqt(pa - pm, pm - tip, axisVector, System.Math.Cos(halfAngle));
                weight = rho / (1 - rho);
            }

        }

    }
}
