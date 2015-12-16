using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
    /// <summary>Class representing a triangular facet (three vertexs)
    /// The triangular facet is used as the underlyind template of the curved triangle
    /// </summary>
    public class Triangle
    {
        /// <summary>The vertexs of the triangle</summary>
        public Vertex[] Vertexs { get { return vertexs; } }
        /// <summary>The sides of the triangle</summary>
        public Side[] Sides { get { return sides; } }
        /// <summary>The face ID of the triangle</summary>
        public int FaceID { get { return faceID; } }
        /// <summary>The facet ID of the trianle</summary>
        public int FacetID { get { return facetID; }  }
        /// <summary> The face type of the triangle</summary>
        public NXOpen.Facet.FacetedFace.FacetedfaceType FaceType { get { return faceType; }  }


        protected Vertex[] vertexs;
        protected Side[] sides;
        protected int faceID;
        protected int facetID;
        protected NXOpen.Facet.FacetedFace.FacetedfaceType faceType;

        /// <summary>Triangle constructor, given three vertexs and three sides</summary>
        /// <param name="vertexs">Array of vertex components</param>
        /// <param name="sides">Array of side components</param>
        /// <param name="faceID">Face ID</param>
        /// <param name="facetID">Facet ID</param>
        /// <param name="faceType">Face Type</param>>
        public Triangle(Vertex[] vertexs, Side[] sides, int facetID, int faceID, NXOpen.Facet.FacetedFace.FacetedfaceType faceType)
        {
            this.vertexs = vertexs;
            this.sides = sides;
            this.facetID = facetID;
            this.faceID = faceID;
            this.faceType = faceType;
        }

        /// <summary>Triangle constructor, given NX-style arrays of data</summary>
        /// <param name="points">Array of point coordinates</param>
        /// <param name="normals">Array of normal components</param>
        /// <param name="faceID">Face ID</param>
        /// <param name="facetID">Facet ID</param>
        /// <param name="faceType">Face Type</param>>
        public Triangle(double[,] points, double[,] normals, int faceID, int facetID, NXOpen.Facet.FacetedFace.FacetedfaceType faceType)
        {
            Snap.InfoWindow.WriteLine(facetID);
            Position p0 = new Position(points[0, 0], points[0, 1], points[0, 2]);
            Position p1 = new Position(points[1, 0], points[1, 1], points[1, 2]);
            Position p2 = new Position(points[2, 0], points[2, 1], points[2, 2]);

            Vector n0 = new Vector(normals[0, 0], normals[0, 1], normals[0, 2]);
            Vector n1 = new Vector(normals[1, 0], normals[1, 1], normals[1, 2]);
            Vector n2 = new Vector(normals[2, 0], normals[2, 1], normals[2, 2]);

            Vertex v0 = new Vertex(p0, n0);
            Vertex v1 = new Vertex(p1, n1);
            Vertex v2 = new Vertex(p2, n2);

            Side s0 = new Side(false, new Snap.Vector(), new Snap.Vector());
            Side s1 = new Side(false, new Snap.Vector(), new Snap.Vector());
            Side s2 = new Side(false, new Snap.Vector(), new Snap.Vector());

            Vertex[] verts = { v0, v1, v2 };
            Side[] sides = { s0, s1, s2 };

            this.vertexs = verts;
            this.faceID = faceID;
            this.facetID = facetID;
            this.faceType = faceType;
            this.sides = sides;
        }

        /// <summary>
        /// Get the tangent vector on the edge
        /// </summary>
        /// <param name="facets"></param>
        /// <param name="facettedBodyTag"></param>
        /// <param name="triangleList"></param>
        public void GetTangentsOnEdge(NXOpen.UF.UFFacet facets, NXOpen.Tag facettedBodyTag, List<Triangle> triangleList)
        {
            int[] adjcntTriangleIDs = new int[3];
            int[] sideIDs = new int[3];

            // Find adjacent three triangles
            for (int i = 0; i < 3; i++)
            {
                facets.AskAdjacentFacet(facettedBodyTag, facetID, (i + 1) % 3, out adjcntTriangleIDs[i], out sideIDs[i]);
                if (adjcntTriangleIDs[i] >= 0)
                {
                    Triangle adjcntTriangle = triangleList.ElementAt(adjcntTriangleIDs[i]);

                    // If the face IDs of the joining triangles are different, there should be an edge
                    if (adjcntTriangle.FaceID != faceID)
                    {
                        GetTangents(i, adjcntTriangle.Vertexs[(sideIDs[i] + 1) % 3].Normal, adjcntTriangle.Vertexs[sideIDs[i] % 3].Normal);
                    }
                }
            }
        }

        /// <summary>
        /// Get the tangent vector, we choose the cross product of the two normals at the same vertex as the tangent
        /// </summary>
        /// <param name="sideID">The ID of the side, should be 0,1 or 2</param>
        /// <param name="N0">The corresponding normal of n0 at the same vertex</param>
        /// <param name="N1">The corresponding normal of n1 at the same vertex</param>
        private void GetTangents(int sideID, Vector N0, Vector N1)
        {

            Position p0 = vertexs[(sideID + 1) % 3].Point;
            Position p1 = vertexs[(sideID + 2) % 3].Point;

            Vector n0 = vertexs[(sideID + 1) % 3].Normal;
            Vector n1 = vertexs[(sideID + 2) % 3].Normal;

            if (Vector.Norm(Vector.Cross(n0, N0)) > 0.01 && Vector.Norm(Vector.Cross(n1, N1)) > 0.01)
            {
                Sides[sideID].IsOnEdge = true;
                Vector tangent0 = Vector.UnitCross(n0, N0);
                if (tangent0 * (p1 - p0) > 0)
                {
                    Sides[sideID].Tangent0 = tangent0;
                }
                else
                {
                    Sides[sideID].Tangent0 = -tangent0;
                }

                Vector tangent1 = Vector.UnitCross(n1, N1);
                if (tangent1 * (p0 - p1) > 0)
                {
                    Sides[sideID].Tangent1 = tangent1;
                }
                else
                {
                    Sides[sideID].Tangent1 = -tangent1;
                }
            }
            else
            {
                Vertexs[(sideID + 1) % 3].Normal = Vector.Unit(n0 + N0);
                Vertexs[(sideID + 2) % 3].Normal = Vector.Unit(n1 + N1);
            }

        }
    }
}
