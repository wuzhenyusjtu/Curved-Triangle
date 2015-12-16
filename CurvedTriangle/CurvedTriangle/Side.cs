using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurvedTriangle
{
    /// <summary>
    /// Class represent one of the sides of a triangle, every side has two tangent vectors at the end points
    /// </summary>
    public class Side
    {
        /// <summary> Indicates whether the side of a triangle is on a edge, edge is where faces of different ids join </summary>
        public bool IsOnEdge { get { return isOnEdge; } set { isOnEdge = value; } }
        /// <summary> Tangent vector at the first vertex on the side </summary>
        public Snap.Vector Tangent0 { get { return tangent0; } set { tangent0 = value; } }
        /// <summary> Tangent vector at the second vertex on the side</summary>
        public Snap.Vector Tangent1 { get { return tangent1; } set { tangent1 = value; } }

        private bool isOnEdge;
        private Snap.Vector tangent0;
        private Snap.Vector tangent1;

        public Side(bool isOnEdge, Snap.Vector tangent0, Snap.Vector tangent1)
        {
            this.isOnEdge = isOnEdge;
            this.tangent0 = tangent0;
            this.tangent1 = tangent1;
        }
    }
}
