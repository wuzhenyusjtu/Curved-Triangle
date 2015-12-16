using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;
using System.Drawing;

namespace CurvedTriangle
{
    /// <summary>
    /// Class representing the dialog that can select the facetted body and build bezier patches.
    /// </summary>
    public class SelectFacetBody : Snap.UI.BlockForm
    {
        /// <summary>the SelectObject block which can select a facetted body</summary>
        private Snap.UI.Block.SelectObject selectFacet;
        /// <summary>The type of triangles the user chooses</summary>
        private Snap.UI.Block.Enumeration triangleType;
        /// <summary>The type of rectangular patches the user chooses</summary>
        private Snap.UI.Block.Enumeration patchScheme;

        /// <summary>Construct a new SelectFaceBody</summary>
        public SelectFacetBody()
        {
           // Note: this dialog doesn't allow us to pick Alice versus PN triangle
           // method for computing the middle point of a cubic triangular patch

            Title = "Planar Facets to Curved Surfaces";
            string cue = "Please select the facetted body";
            string label = "Select facetted body";

            // create a selection block
            selectFacet = new Snap.UI.Block.SelectObject(cue, label);
            Snap.NX.ObjectTypes.Type[] types = { Snap.NX.ObjectTypes.Type.FacettedModel };
            selectFacet.SetFilter(types);
            selectFacet.AllowMultiple = true;
            selectFacet.StepStatus = Snap.UI.Block.StepStatus.Required;

            // create the eumeration blocks for triangle type
            triangleType = new Snap.UI.Block.Enumeration();
            triangleType.Label = "Type of Triangles";
            triangleType.Items = new string[3] { "Cubic (Alice)", "Quadratic (Rational)", "Mixed (Quad, Cubic)" };
            triangleType.PresentationStyle = Snap.UI.Block.EnumPresentationStyle.RadioBox;
            triangleType.Layout = Snap.UI.Block.Layout.Horizontal;

            // create tge enumeration blocks for patch scheme
            patchScheme = new Snap.UI.Block.Enumeration();
            patchScheme.Label = "Type of Rectangular Patches";
            patchScheme.Items = new string[2] { "Packed (Three)", "Pinched (One)" };
            patchScheme.PresentationStyle = Snap.UI.Block.EnumPresentationStyle.RadioBox;
            patchScheme.Layout = Snap.UI.Block.Layout.Horizontal;

            // add five blocks to the BlockForm
            AddBlocks(selectFacet, triangleType, patchScheme);
        }

        /// <summary>
        /// This function will be called if the user click on the Apply button in my dialog.
        /// NX will create the bezier patches using the methods that the user chooses.
        /// </summary>
        public override void OnApply()
        {
            Snap.NX.NXObject[] bodies = selectFacet.SelectedObjects;

            int numBodies = bodies.Length;
            for (int i = 0; i < numBodies; i++)
            {
                NXOpen.Facet.FacetedBody selectedBody = (NXOpen.Facet.FacetedBody)bodies[i];

                TriangleType selectedTriangleType;
                if (triangleType.SelectedIndex == 0)
                {
                    selectedTriangleType = TriangleType.Cubic;
                }
                else if (triangleType.SelectedIndex == 1)
                {
                    selectedTriangleType = TriangleType.Quadratic;
                }
                else
                {
                    selectedTriangleType = TriangleType.Mixed;
                }

                RectangularPatchScheme selectedPatchScheme;
                if (patchScheme.SelectedIndex == 0)
                {
                    selectedPatchScheme = RectangularPatchScheme.Packed;
                }
                else
                {
                    selectedPatchScheme = RectangularPatchScheme.Pinched;
                }

                CreateCurvedTriangles(selectedBody, selectedTriangleType, selectedPatchScheme);
            }
            base.OnApply();

        }

        /// <summary>Gets curved-triangles from an NX facetted body</summary>
        /// <param name="body">Facetted body whose facets we want to get</param>
        /// <returns>List of curved-triangular facets comprising the selected bodies</returns>
        private List<CubicCurvedTriangle> GetCubicCurvedTriangles(NXOpen.Facet.FacetedBody body)
        {
            Snap.Globals.NXOpenWorkPart.FacetedBodies.Convert(body, NXOpen.Facet.FacetedBodyCollection.Type.Jt);

            // Get faces of the faceted body
            NXOpen.Facet.FacetedFace[] faces = body.GetFaces();

            // Get facets
            NXOpen.UF.UFSession ufs = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.UFFacet myFacets = ufs.Facet;
            
            // Get a list containing triangles, which are underlying templates for Bezier curved triangles
            List<Triangle> triangleList = GetTriangleList(body, myFacets, faces);

            // Convert to NX format
            Snap.Globals.NXOpenWorkPart.FacetedBodies.Convert(body, NXOpen.Facet.FacetedBodyCollection.Type.Nx);
            NXOpen.Tag NXBodyTag = body.Tag;

            List<CubicCurvedTriangle> curvedTriangleList = new List<CubicCurvedTriangle>();

            //Cycle through all the triangle templates, creating Bezier curved triangles according their face type
            foreach (Triangle triangle in triangleList)
            {
                switch (triangle.FaceType)
                {
                    // Special case for cylinderical face
                    case NXOpen.Facet.FacetedFace.FacetedfaceType.Cylindrical:
                        CylCubicTriangle cylinderCubicTriangle = new CylCubicTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType);
                        curvedTriangleList.Add(cylinderCubicTriangle);
                        break;

                    // Special case for conical face
                    case NXOpen.Facet.FacetedFace.FacetedfaceType.Conical:
                        ConeCubicTriangle coneCubicTriangle = new ConeCubicTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType, faces[triangle.FaceID]);
                        curvedTriangleList.Add(coneCubicTriangle);
                        break;
                    
                    // Special case for spherical face
                    case NXOpen.Facet.FacetedFace.FacetedfaceType.Spherical:
                        SphCubicTriangle sphereCubicTriangle = new SphCubicTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType);
                        curvedTriangleList.Add(sphereCubicTriangle);
                        break;

                    // Default case for sphere-like face
                    default:
                        SphCubicTriangle defaultCubicTriangle = new SphCubicTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType);
                        curvedTriangleList.Add(defaultCubicTriangle);
                        break;
                }
            }
            return curvedTriangleList;
        }

        /// <summary>Gets quadratic triangles from an NX facetted body</summary>
        /// <param name="body">Facetted body whose facets we want to get</param>
        /// <returns>List of quadratic triangular facets comprising the selected bodies</returns>
        private List<QuadCurvedTriangle> GetQuadCurvedTriangles(NXOpen.Facet.FacetedBody body)
        {
            Snap.Globals.NXOpenWorkPart.FacetedBodies.Convert(body, NXOpen.Facet.FacetedBodyCollection.Type.Jt);
            NXOpen.Facet.FacetedFace[] faces = body.GetFaces();

            // Get facets
            NXOpen.UF.UFSession ufs = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.UFFacet myFacets = ufs.Facet;

            // Get a list containing triangles, which are underlying templates for Bezier curved triangles
            List<Triangle> triangleList = GetTriangleList(body, myFacets, faces);

            // Convert to NX format
            Snap.Globals.NXOpenWorkPart.FacetedBodies.Convert(body, NXOpen.Facet.FacetedBodyCollection.Type.Nx);
            NXOpen.Tag NXBodyTag = body.Tag;

            List<QuadCurvedTriangle> QuadCurvedTriangleList = new List<QuadCurvedTriangle>();

            //Cycle through all the triangle templates, creating Bezier curved triangles according their face type
            foreach (Triangle triangle in triangleList)
            {
                switch (triangle.FaceType)
                {
                    // Special case for cylindrical face
                    case NXOpen.Facet.FacetedFace.FacetedfaceType.Cylindrical:
                        CylQuadTriangle cylinderQuadraticTriangle = new CylQuadTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType);
                        QuadCurvedTriangleList.Add(cylinderQuadraticTriangle);
                        break;

                    // Special case for conial face
                    case NXOpen.Facet.FacetedFace.FacetedfaceType.Conical:
                        ConeQuadTriangle coneQuadraticTriangle = new ConeQuadTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType, faces[triangle.FaceID]);
                        QuadCurvedTriangleList.Add(coneQuadraticTriangle);
                        break;

                    // Special case for spherical face
                    case NXOpen.Facet.FacetedFace.FacetedfaceType.Spherical:
                        SphQuadTriangle sphereQuadraticTriangle = new SphQuadTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType);
                        QuadCurvedTriangleList.Add(sphereQuadraticTriangle);
                        break;

                    // Default case for sphere-like face
                    default:
                        SphQuadTriangle defaultQuadraticTriangle = new SphQuadTriangle(triangle.Vertexs, triangle.Sides, triangle.FacetID, triangle.FaceID, triangle.FaceType);
                        QuadCurvedTriangleList.Add(defaultQuadraticTriangle);
                        break;
                }
                // Get the next facet
            }
            return QuadCurvedTriangleList;
        }

        private List<Triangle> GetTriangleList(NXOpen.Facet.FacetedBody body, NXOpen.UF.UFFacet myFacets, NXOpen.Facet.FacetedFace[] faces)
        {

            List<Triangle> triangleList = new List<Triangle>();

            // Convert to JT format
            NXOpen.Tag JTBodyTag = body.Tag;
            int facetID = NXOpen.UF.UFConstants.UF_FACET_NULL_FACET_ID;

            // Initialise for cycling 
            myFacets.CycleFacets(JTBodyTag, ref facetID);  
            // Cycle through all facets, in order to create underlying triangle templates for Bezier curved triangles
            while (facetID != NXOpen.UF.UFConstants.UF_FACET_NULL_FACET_ID)
            {
                int numVerts; ;
                myFacets.AskNumVertsInFacet(JTBodyTag, facetID, out numVerts);

                double[,] points = new double[numVerts, 3];
                double[,] normals = new double[numVerts, 3];

                // Get points and normals
                myFacets.AskVerticesOfFacet(JTBodyTag, facetID, out numVerts, points);
                myFacets.AskNormalsOfFacet(JTBodyTag, facetID, out numVerts, normals);

                // Get faceID
                int faceID;
                myFacets.AskFaceIdOfFacet(JTBodyTag, facetID, out faceID);

                Triangle triangle = new Triangle(points, normals, faceID, facetID, faces[faceID].FaceType);

                triangleList.Add(triangle);
                myFacets.CycleFacets(JTBodyTag, ref facetID);  // Get the next facet
            }

            // Check if the distance between points of the triangle is large enough to build a curved triangle
            triangleList.RemoveAll(SmallToBuild);
            
            // Convert to NX format
            Snap.Globals.NXOpenWorkPart.FacetedBodies.Convert(body, NXOpen.Facet.FacetedBodyCollection.Type.Nx);
            NXOpen.Tag NXBodyTag = body.Tag;

            // Cycle through each triangle
            // If any side of a triangle is on a edge(where two different faces join), get tangent vectors as the cross product of two normals
            foreach (Triangle triangle in triangleList)
            {
                triangle.GetTangentsOnEdge(myFacets, NXBodyTag, triangleList);
            }

            return triangleList;
        }

        /// <summary>
        /// Create the curved triangles according to the user's requirements
        /// </summary>
        /// <param name="selectedBody">the faceted body that the user select</param>
        /// <param name="triangleType">>the type of triangles the user chooses</param>
        /// <param name="patchScheme">The scheme of rectangles patches the user chooses</param>
        private void CreateCurvedTriangles(NXOpen.Facet.FacetedBody selectedBody, TriangleType triangleType, RectangularPatchScheme patchScheme)
        {
            #region Case when type == cubic && scheme == pinched
            if (triangleType == TriangleType.Cubic && patchScheme == RectangularPatchScheme.Pinched)
            {
                List<CubicCurvedTriangle> cubicTriangleList = GetCubicCurvedTriangles(selectedBody);
                ConvertToRectangularPatch converter = new ConvertToRectangularPatch();

                // A list containing rectangular patches
                // In order to put Bezier curved triangle to NX, we need to convert it to rectangular patch
                List<Snap.NX.Bsurface> rectPatches = new List<Snap.NX.Bsurface>();

                // Convert Bezier curved triangle to pinched rectangular patches 
                foreach (CubicCurvedTriangle cubicTriangle in cubicTriangleList)
                {
                    Snap.NX.Bsurface face = converter.GetCubicPinchedRectPatches(cubicTriangle.CubicPoles);
                    rectPatches.Add(face);
                }
            }
            #endregion

            #region Case when type == cubic && scheme == packed
            else if (triangleType == TriangleType.Cubic && patchScheme == RectangularPatchScheme.Packed)
            {
                List<CubicCurvedTriangle> cubicTriangleList = GetCubicCurvedTriangles(selectedBody);
                ConvertToRectangularPatch converter = new ConvertToRectangularPatch();

                // A list containing rectangular patches
                // In order to put Bezier curved triangle to NX, we need to convert it to rectangular patch
                List<Snap.NX.Bsurface> rectPatches = new List<Snap.NX.Bsurface>();
                foreach (CubicCurvedTriangle cubicTriangle in cubicTriangleList)
                {
                    // Convert Bezier curved triangle to packed rectangular patches 
                    Snap.NX.Bsurface[] bs = converter.GetCubicPackedRectPatches(cubicTriangle.CubicPoles);
                    rectPatches.Add(bs[0]);
                    rectPatches.Add(bs[1]);
                    rectPatches.Add(bs[2]);
                }
            }
            #endregion

            #region Case when type == quadratic && scheme == pinched
            else if (triangleType == TriangleType.Quadratic && patchScheme == RectangularPatchScheme.Pinched)
            {
                List<QuadCurvedTriangle> quadraticTriangleList = GetQuadCurvedTriangles(selectedBody);
                ConvertToRectangularPatch converter = new ConvertToRectangularPatch();

                // A list containing rectangular patches
                // In order to put Bezier curved triangle to NX, we need to convert it to rectangular patch
                List<Snap.NX.Bsurface> rectPatches = new List<Snap.NX.Bsurface>();
                foreach (QuadCurvedTriangle quadraticTriangle in quadraticTriangleList)
                {
                    // convert triangle to Bezier patches
                    Snap.NX.Bsurface face = converter.GetQuadraticPinchedRectPatch(quadraticTriangle.QuadPoles);
                    rectPatches.Add(face);
                }
            }
            #endregion

            #region Case when type == mixed && scheme == pinched
            else if (triangleType == TriangleType.Mixed && patchScheme == RectangularPatchScheme.Pinched)
            {
                List<QuadCurvedTriangle> quadraticTriangleList = GetQuadCurvedTriangles(selectedBody);
                ConvertToRectangularPatch converter = new ConvertToRectangularPatch();

                // A list containing rectangular patches
                // In order to put Bezier curved triangle to NX, we need to convert it to rectangular patch
                List<Snap.NX.Bsurface> rectPatches = new List<Snap.NX.Bsurface>();
                foreach (QuadCurvedTriangle quadraticTriangle in quadraticTriangleList)
                {
                    // Convert Bezier curved triangle to pinched rectangular patches 

                    // Check if inflexion points exist, if they do, construct cubic curved triangle instead 
                    if (quadraticTriangle.AnyInflexionPt() == false)
                    {
                        // Contruct quadratic curved triangle
                        Snap.NX.Bsurface face = converter.GetQuadraticPinchedRectPatch(quadraticTriangle.QuadPoles);
                        rectPatches.Add(face);
                    }
                    else
                    {
                        // Contruct cubic curved triangle
                        CubicTrianglePoles cubicPoles = quadraticTriangle.ConvertToCubic();
                        Snap.NX.Bsurface face = converter.GetCubicPinchedRectPatches(cubicPoles);
                        rectPatches.Add(face);
                    }
                }
            }
            #endregion

            else
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Check if the triangle is too small to build a curved triangle
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        private bool SmallToBuild(Triangle triangle)
        {
            Position p1 = triangle.Vertexs[0].Point;
            Position p2 = triangle.Vertexs[1].Point;
            Position p3 = triangle.Vertexs[2].Point;

            if (Position.Distance(p1, p2) < 0.0003 || Position.Distance(p2, p3) < 0.0003 || Position.Distance(p1, p3) < 0.0003)
            {
                return true;
            }
            else return false;
        }

    }
}
