using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    /// <summary>Class representing a vertex (point and normal)</summary>
    public class Vertex
    {
        /// <summary>The location of the vertex</summary>
        public Snap.Position Point { get { return point; } set { point = value; } }

        /// <summary>The normal at the vertex</summary>
        public Snap.Vector Normal { get { return normal; } set { normal = value; } }

        private Snap.Position point;
        private Snap.Vector normal;

        public Vertex(Snap.Position point, Snap.Vector normal)
        {
            this.point = point;
            this.normal = normal;
        }
    }
}
