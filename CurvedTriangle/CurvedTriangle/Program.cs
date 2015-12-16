using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
   /// <summary>The type (degree) of the triangles</summary>
    public enum TriangleType
    {
       /// <summary>Degree = 2</summary>
        Quadratic = 2,

       /// <summary>Degree = 3</summary>
        Cubic = 3,

       /// <summary>Degree = 2, unless inflexion, in which case degree 3</summary>
        Mixed = 4
    };

   /// <summary>Method of converting triangular patch to rectangular ones</summary>
    public enum RectangularPatchScheme
    {
       /// <summary>
       /// Three rectangular patches for each triangle
       /// </summary>
        Packed = 0,

       /// <summary>
       /// One rectangular patch per triangle, with a singularity
       /// </summary>
        Pinched = 1
    };

   class Program
   {
      static void Main(string[] args)
      {
        // For debug purpose
         System.Diagnostics.Debugger.Launch();
         SelectFacetBody myDialog = new SelectFacetBody();
         myDialog.Show();
      }

      public static int GetUnloadOption(string dummy)
      {
         return (int)Snap.UnloadOption.Immediately;
      }
   }
}
