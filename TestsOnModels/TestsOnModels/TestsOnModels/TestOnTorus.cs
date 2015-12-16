using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsOnModels
{
    class TestOnTorus : TestOnModel
    {
        /// <summary>Number of tests we want to carry out on this model/// </summary>
        private int testsCount;
        private Snap.NX.Face face;

        public TestOnTorus(Snap.Position center, Snap.Vector axis, double majorRadius, double minorRadius, int testsCount)
        {
            Snap.NX.Torus torus = Snap.Create.Torus(center, axis, majorRadius, minorRadius);
            face = torus.Faces[0];
            this.testsCount = testsCount;

            StartTest();
        }

        private void StartTest()
        {
            // Turn off Update
            NXOpen.UF.UFSession ufs = NXOpen.UF.UFSession.GetUFSession();
            ufs.Disp.SetDisplay(NXOpen.UF.UFConstants.UF_DISP_SUPPRESS_DISPLAY);

            #region Create Quadratic & Cubic Curved Triangles to Test
            for (int k = 6; k < testsCount+6; k++)
            {
                int facetsCount = 2 * k * k;

                //Get Points & Normals on parametric space, store them in vertexs
                Vertex[,] vertexs = new Vertex[k + 1, k + 1];
                GetVertexsOnFace(vertexs, face, k);

                // Get triangle list by tessellation of the face
                List<Triangle> triangleList = new List<Triangle>();
                GetTriangleList(vertexs, triangleList, k);
                ConvertToRectangularPatch converter = new ConvertToRectangularPatch();

                #region Quadratic Case

                Snap.InfoWindow.WriteLine("Quadratic:   " + facetsCount.ToString());

                List<QuadCurvedTriangle> quadTriangleList = GetQuadCurvedTriangles(triangleList);

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

                GetDistanceDeviation(patchfacesQuad.ToArray(), face);

                Snap.NX.Sew quadCurvedTorusSew = Snap.Create.Sew(rectPatchesQuad[0], rectPatchesQuad.GetRange(1, rectPatchesQuad.Count - 1).ToArray());
                GetAngleDeviation(quadCurvedTorusSew.Body.Edges);

                quadCurvedTorusSew.Delete();
                Snap.NX.NXObject.Delete(rectPatchesQuad.ToArray());
                #endregion

                #region Cubic Case

                Snap.InfoWindow.WriteLine("Cubic:   " + facetsCount.ToString());

                List<CubicCurvedTriangle> cubicTriangleList = GetCubicCurvedTriangles(triangleList);

                List<Snap.NX.Bsurface> rectPatchesCubic = new List<Snap.NX.Bsurface>();

                foreach (CubicCurvedTriangle cubicTriangle in cubicTriangleList)
                {
                    // convert triangle to Bezier patches

                    Snap.NX.Bsurface rectPatch = converter.GetCubicPinchedRectPatches(cubicTriangle.CubicPoles);
                    rectPatchesCubic.Add(rectPatch);
                }
                List<Snap.NX.Face> patchfacesCubic = new List<Snap.NX.Face>();
                foreach (Snap.NX.Bsurface patch in rectPatchesCubic)
                {
                    patchfacesCubic.Add(patch.Face);
                }

                GetDistanceDeviation(patchfacesCubic.ToArray(), face);

                Snap.NX.Sew cubicCurvedTorusSew = Snap.Create.Sew(rectPatchesCubic[0], rectPatchesCubic.GetRange(1, rectPatchesCubic.Count - 1).ToArray());
                GetAngleDeviation(cubicCurvedTorusSew.Body.Edges);

                cubicCurvedTorusSew.Delete();
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
            // UV value in angle
            Snap.Position pointOnFace = face.Position(uv);

            // UV value in radian 
            double u = uv[0] / 180 * System.Math.PI;
            double v = uv[1] / 180 * System.Math.PI;
            uv = new double[2] { u, v };
            NXOpen.UF.UFSession ufs = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.ModlSrfValue surfaceValues = new NXOpen.UF.ModlSrfValue();
            int mode = NXOpen.UF.UFConstants.UF_MODL_EVAL_UNIT_NORMAL;
            ufs.Modl.EvaluateFace(face.NXOpenTag, mode, uv, out surfaceValues);
            Snap.Vector unitNorm = surfaceValues.srf_unormal;
            return new Vertex(pointOnFace, unitNorm);
        }
    }
}
