using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle.ClosestPointQuadTriangle
{

    /// <summary>
    /// George' method to get the inner control point for a Bezier quadratic curve
    /// On the intersection line of the perpedicular planes derived from the start point and the end point, we use the point closest to the coord as the innter control point
    /// Our test results shows that this method performs worse than the intersection point method
    /// </summary>
    class ClosestPtQuadTriangle
    {
        /// <summary> figure out the weight of the mid pole </summary>
        /// <param name="p1"> the first position of the curve </param>
        /// <param name="p2"> the second position of the curve </param>
        /// <param name="n1"> the first normal of the curve </param>
        /// <param name="n2"> the second normal of the curve </param>
        /// <param name="p12"> the mid pole</param>
        /// <param name="weight"> the weight of the mid pole </param>
        public void QuadraticFromPtsNormals(Position p1, Position p2, Vector n1, Vector n2, out Position p12, out double weight)
        {
            // Apex point between q0 and q1
            Vector c = p2 - p1;
            Vector b = Vector.Cross(n1, c);
            Vector h = Vector.UnitCross(b, n1);
            c = p1 - p2;
            b = Vector.Cross(n2, c);
            Vector k = Vector.UnitCross(b, n2);

            // Get the intersection line of the two planes, derived from the normals at the start and end point
            Snap.Geom.Surface.Plane plane1 = new Snap.Geom.Surface.Plane(p1, n1);
            Snap.Geom.Surface.Plane plane2 = new Snap.Geom.Surface.Plane(p2, n2);
            Snap.Geom.Curve.Ray intersectRay = Snap.Compute.Intersect(plane1, plane2);

            // Get the coord ray
            Snap.Geom.Curve.Ray baseRay = new Snap.Geom.Curve.Ray(p1, new Snap.Vector(p2 - p1));
            if (Vector.Angle(h, k) > 179)
            {
                p12 = (p1 + p2) / 2;
                weight = 1;
            }
            else
            {
                // Get closest point
                Compute.DistanceResult result = Snap.Compute.ClosestPoints(intersectRay, baseRay);
                p12 = result.Point1;
                weight = System.Math.Sqrt((1 - h * k) / 2);
            }
        }

    }
}
