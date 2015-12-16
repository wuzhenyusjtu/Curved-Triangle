using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
    /// <summary>Class representing a cubic curved triangle (three vertexs and ten bezier control points)</summary>
    abstract class CubicCurvedTriangle : Triangle
    {

        protected CubicTrianglePoles cubicPoles;

        /// <summary>The vertexs of the triangle</summary>
        /// <summary> Ten control points of the curved cubic triangle </summary>
        ///<summary> Notation is from Peters/Vlachos paper </summary>
        public CubicTrianglePoles CubicPoles { get { return cubicPoles; } set { cubicPoles = value; } }

        /// <summary> Constructor for the cubic curved triangle, given the poles </summary>
        /// <param name="vertexs"> The vertexs of the triangle template</param>
        /// <param name="facetID"> The facetID of the triangle template</param>
        /// <param name="faceID"> The faceID of the triangle template</param>
        /// <param name="faceType"> The type of face of the triangle template</param>
        /// <param name="cubicTrianglePoles"> Ten poles of the cubic curved triangle</param>
        public CubicCurvedTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID,
            NXOpen.Facet.FacetedFace.FacetedfaceType faceType, CubicTrianglePoles cubicPoles) 
            :base(vertexs, sides, facetID, faceID, faceType)
        {
            this.cubicPoles = cubicPoles;
        }

        /// <summary>
        /// overloaded Constructor for the cubic curved triangle (no poles input)
        /// </summary>
        /// <param name="vertexs"> The vertexs of the triangle template</param>
        /// <param name="facetID"> The facetID of the triangle template</param>
        /// <param name="faceID"> The faceID of the triangle template </param>
        /// <param name="faceType"> The type of face of the triangle template</param>
        public CubicCurvedTriangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID, NXOpen.Facet.FacetedFace.FacetedfaceType faceType)
           : base(vertexs, sides, facetID, faceID, faceType)
        {
        }

        abstract protected void GetCubicTrianglePoles();

        abstract protected Position[] CubicFromPtsNormals(Position p1, Position p2, Vector n1, Vector n2);

        /// <summary>
        /// Get the mid (#10) pole of the cubic curved triangle
        /// </summary>
        /// <param name="marginPoles">The poles on the sides of the cubic curved triangle</param>
        /// <param name="vertexs"></param>
        /// <returns>The mid pole</returns>
        abstract protected Position GetMidPoleAlice();

       /// <summary>
       /// Construct a cubic curve from two points and two tangents
       /// </summary>
       /// <remarks>
       /// Used for triangle edges that lie along face edges (so we have a tangent direction)
       /// </remarks>
       /// <param name="p0">Start point</param>
       /// <param name="p1"></param>
       /// <param name="t0">Start tangent (unit vector)</param>
       /// <param name="t1"></param>
       /// <returns></returns>
        protected Position[] CubicFromTangents(Position p0, Position p1, Vector t0, Vector t1)
        {
            Vector c = Vector.Unit(p1 - p0);
            double k = Vector.Norm(p1 - p0);
            double cosAlpha0 = c * t0;
            double cosAlpha1 = (-c) * t1;

            double d0 = (2 * k) / (3 * (1 + cosAlpha1));
            double d1 = (2 * k) / (3 * (1 + cosAlpha0));

            Position pa = p0 + d0 * t0;
            Position pb = p1 + d1 * t1;

            Position[] poles =  { pa, pb };
            return poles;
        }
    }
}
