using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
    class CylQuadTriangle : QuadCurvedTriangle
    {

        public CylQuadTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID,
            NXOpen.Facet.FacetedFace.FacetedfaceType faceType)
            :base(vertexs, sides, facetID, faceID, faceType)
        {
            quadPoles = new QuadTrianglePoles();
            GetSixPolesWithWeights();
        }

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

        /// <summary> figure out the weight of the mid pole for cylinder </summary>
        /// <param name="p1"> the first position of the curve </param>
        /// <param name="p2"> the second position of the curve </param>
        /// <param name="n1"> the first normal of the curve </param>
        /// <param name="n2"> the second normal of the curve </param>
        /// <param name="p12"> the mid pole</param>
        /// <param name="weight"> the weight of the mid pole </param>
        protected override void QuadraticFromPtsNormals(Position p1, Position p2, Vector n1, Vector n2, out Position p12, out double weight)
        {
            Compute.DistanceResult result = null;
            double angle = Vector.Angle(n1, n2);
            if (angle < 0.01)
            {
                p12 = (p1 + p2) / 2;
                weight = 1;
            }
            else
            {
                weight = System.Math.Cos(angle * System.Math.PI / 360);
                Vector c = p2 - p1;
                Vector b = Vector.Cross(n1 + n2, c);
                Vector h = Vector.UnitCross(b, n1);
                Vector k = Vector.UnitCross(-b, n2);
                if (Vector.Angle(h, k) > 179)
                {
                    p12 = (p1 + p2) / 2;
                }
                else
                {
                    result = Snap.Compute.ClosestPoints(new Snap.Geom.Curve.Ray(p1, h), new Snap.Geom.Curve.Ray(p2, k));
                    p12 = (result.Point1 + result.Point2) / 2;
                }
            }
        }
    }
}
