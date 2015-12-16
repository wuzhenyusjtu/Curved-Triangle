using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    class TestOnHump : TestOnModel
    {
        /// <summary>Number of tests we want to carry out on this model/// </summary>
        private int testsCount = 22;
        private Snap.NX.Face face;

        public TestOnHump(Snap.NX.Face face, int testsCount)
        {
            this.face = face;
            this.testsCount = testsCount;

            StartTest();
        }

        private void StartTest()
        { 
            // Turn off Update
            NXOpen.UF.UFSession ufs = NXOpen.UF.UFSession.GetUFSession();
            ufs.Disp.SetDisplay(NXOpen.UF.UFConstants.UF_DISP_SUPPRESS_DISPLAY);

            #region Create Quadratic & Cubic Curved Triangles to Test
            for (int k = 2; k < testsCount+2; k++)
            {

                int facetsCount = k * k * 2;

                //Get Points & Normals on parametric space, store them in vertexs
                Vertex[,] vertexs = new Vertex[k + 1, k + 1];
                GetVertexsOnFace(vertexs, face, k);

                // Get triangle list by tessellation of the face
                List<Triangle> triangleList = new List<Triangle>();
                GetTriangleList(vertexs, triangleList, k);

                ConvertToRectangularPatch converter = new ConvertToRectangularPatch();

                #region Quadratic Case

                Snap.InfoWindow.WriteLine("Quadratic:   " + facetsCount.ToString());
                // Get Quadratic Triangle list
                List<QuadCurvedTriangle> quadTriangleList = GetQuadCurvedTriangles(triangleList);

                // List where rectangular patches are stored
                List<Snap.NX.Bsurface> rectPatchesQuad = new List<Snap.NX.Bsurface>();

                foreach (QuadCurvedTriangle quadTriangle in quadTriangleList)
                {
                    // Convert Bezier curved triangle to pinched rectangular patches 

                    // Check if inflexion points exist, if they do, construct cubic curved triangle instead 
                    if (quadTriangle.AnyInflexionPt() == false)
                    {
                        // Contruct quadratic curved triangle
                        Snap.NX.Bsurface rectPatch = converter.GetQuadraticPinchedRectPatch(quadTriangle.QuadPoles);
                        rectPatchesQuad.Add(rectPatch);
                    }
                    else
                    {
                        // Contruct cubic curved triangle
                        CubicTrianglePoles cubicPoles = quadTriangle.ConvertToCubic();
                        Snap.NX.Bsurface rectPatch = converter.GetCubicPinchedRectPatches(cubicPoles);
                        rectPatchesQuad.Add(rectPatch);
                    }
                }
                List<Snap.NX.Face> patchfacesQuad = new List<Snap.NX.Face>();
                foreach (Snap.NX.Bsurface patch in rectPatchesQuad)
                {
                    patchfacesQuad.Add(patch.Face);
                }

                // Calculate Maximum Distance Deviation, Print it in InfoWindow
                GetDistanceDeviation(patchfacesQuad.ToArray(), face);

                // Calculate Maximum Angle Deviation, Print it in InfoWindow
                // Sew the rectangular patches first
                Snap.NX.Sew sewRectPatchesQuad = Snap.Create.Sew(rectPatchesQuad[0], rectPatchesQuad.GetRange(1, rectPatchesQuad.Count - 1).ToArray());
                GetAngleDeviation(sewRectPatchesQuad.Body.Edges);

                // Delete NX Objects
                sewRectPatchesQuad.Delete();
                Snap.NX.NXObject.Delete(rectPatchesQuad.ToArray());
                #endregion

                #region Cubic Case

                Snap.InfoWindow.WriteLine("Cubic:   " + facetsCount.ToString());

                // Get Cubic Triangle list
                List<CubicCurvedTriangle> cubicTriangleList = GetCubicCurvedTriangles(triangleList);

                // List where rectangular patches are stored
                List<Snap.NX.Bsurface> rectPatchesCubic = new List<Snap.NX.Bsurface>();

                foreach (CubicCurvedTriangle cubicTriangle in cubicTriangleList)
                {
                    Snap.NX.Bsurface rectPatch = converter.GetCubicPinchedRectPatches(cubicTriangle.CubicPoles);
                    rectPatchesCubic.Add(rectPatch);
                }
                List<Snap.NX.Face> patchfaces = new List<Snap.NX.Face>();
                foreach (Snap.NX.Bsurface patch in rectPatchesCubic)
                {
                    patchfaces.Add(patch.Face);
                }

                // Calculate Maximum Distance Deviation, Print it in InfoWindow
                GetDistanceDeviation(patchfaces.ToArray(), face);

                // Calculate Maximum Angle Deviation, Print it in InfoWindow
                // Sew the rectangular patches first
                Snap.NX.Sew sewRectPatchesCubic = Snap.Create.Sew(rectPatchesCubic[0], rectPatchesCubic.GetRange(1, rectPatchesCubic.Count - 1).ToArray());
                GetAngleDeviation(sewRectPatchesCubic.Body.Edges);

                // Delete NX Objects
                sewRectPatchesCubic.Delete();
                Snap.NX.NXObject.Delete(rectPatchesCubic.ToArray());
                #endregion
            }
            #endregion

            //Turn on Update
            ufs.Disp.SetDisplay(NXOpen.UF.UFConstants.UF_DISP_UNSUPPRESS_DISPLAY);
            ufs.Disp.RegenerateDisplay();
        }

        protected override void GetVertexsOnFace(Vertex[,] vertexs, Snap.NX.Face face, int k)
        {

            Snap.Geom.Box2d box = face.BoxUV;
            double u, v;
            double deltaU = (box.MaxU - box.MinU) / k;
            double deltaV = (box.MaxV - box.MinV) / k;
            for (int i = 0; i <= k; i++)
            {
                for (int j = 0; j <= k; j++)
                {
                    u = box.MinU + i * deltaU;
                    v = box.MinV + j * deltaV;
                    double[] uv = new double[] { u, v };
                    Vertex vertex = GetNormsPtsInParamSpace(face, uv);
                    vertexs[i, j] = vertex;
                }
            }
        }

        protected override Vertex GetNormsPtsInParamSpace(Snap.NX.Face face, params double[] uv)
        {
            NXOpen.UF.UFSession ufs = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.ModlSrfValue surfaceValues = new NXOpen.UF.ModlSrfValue();
            int mode = NXOpen.UF.UFConstants.UF_MODL_EVAL_UNIT_NORMAL;
            ufs.Modl.EvaluateFace(face.NXOpenTag, mode, uv, out surfaceValues);
            Snap.Vector unitNorm = surfaceValues.srf_unormal;
            Snap.Position pointOnFace = face.Position(uv);
            return new Vertex(pointOnFace, unitNorm);
        }
    }
}
