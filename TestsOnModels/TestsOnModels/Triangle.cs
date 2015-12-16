using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    /// <summary>Class representing a triangular facet (three vertices)</summary>
    public class Triangle
    {
        /// <summary>The vertexs of the triangle</summary>
        public Vertex[] Vertexs { get { return vertexs; } }

        protected Vertex[] vertexs;

        /// <summary>Triangle constructor, given three vertices</summary>
        /// <param name="v0">First vertex</param>
        /// <param name="v1">Second vertex</param>
        /// <param name="v2">Third vertex</param>
        /// <param name="faceID">Face ID</param>
        /// <param name="facetID">Facet ID</param>
        public Triangle(Vertex[] vertexs)
        {
            this.vertexs = vertexs;
        }

        /// <summary>Triangle constructor, given NX-style arrays of data</summary>
        /// <param name="points">Array of point coordinates</param>
        /// <param name="normals">Array of normal components</param>
        /// <param name="faceID">Face ID</param>
        /// <param name="facetID">Facet ID</param>
        public Triangle(Snap.Position[] points, Snap.Vector[] normals)
        {
            Snap.Position p0 = new Snap.Position(points[0]);
            Snap.Position p1 = new Snap.Position(points[1]);
            Snap.Position p2 = new Snap.Position(points[2]);

            Snap.Vector n0 = new Snap.Vector(normals[0]);
            Snap.Vector n1 = new Snap.Vector(normals[1]);
            Snap.Vector n2 = new Snap.Vector(normals[2]);

            Vertex v0 = new Vertex(p0, n0);
            Vertex v1 = new Vertex(p1, n1);
            Vertex v2 = new Vertex(p2, n2);

            Vertex[] verts = { v0, v1, v2 };

            this.vertexs = verts;
        }

    }
}
