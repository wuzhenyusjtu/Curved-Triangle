using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TestsOnModels
{
    class TestOnSphere : TestOnModel
    {
        /// <summary>Number of tests we want to carry out on this model/// </summary>
        private int testsCount;
        private Snap.NX.Face[] faces;
        private double radius;
        private Snap.Position center;

        public TestOnSphere(Snap.Position center, double radius, int testsCount)
        {
            faces = GetBezierSphereFaces(center, radius);
            this.center = center;
            this.radius = radius;
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
                int facetsCount = 12 * k * k;

                // Cycle through six faces of a Bezier Sphere patch
                List<Triangle> triangleList = new List<Triangle>();
                foreach (Snap.NX.Face face in faces)
                {
                    //Get Points & Normals on parametric space, store them in vertexs
                    Vertex[,] vertexs = new Vertex[k + 1, k + 1];
                    GetVertexsOnFace(vertexs, face, k);

                    // Get triangle list by tessellation of the face
                    GetTriangleList(vertexs, triangleList, k);
                }

                ConvertToRectangularPatch converter = new ConvertToRectangularPatch();

                #region Quadratic Case

                Snap.InfoWindow.WriteLine("Quadratic:   " + facetsCount.ToString());

                List<QuadCurvedTriangle> quadTriangleList = GetQuadCurvedTriangles(triangleList);

                List<Snap.NX.Bsurface> rectPatchesQuad = new List<Snap.NX.Bsurface>();
                foreach (QuadCurvedTriangle quadTriangle in quadTriangleList)
                {
                    Snap.NX.Bsurface rectPatch = converter.GetQuadraticPinchedRectPatch(quadTriangle.QuadPoles);
                    rectPatchesQuad.Add(rectPatch);
                }

                List<Snap.NX.Face> patchfacesQuad = new List<Snap.NX.Face>();
                foreach (Snap.NX.Bsurface patch in rectPatchesQuad)
                {
                    patchfacesQuad.Add(patch.Face);
                }
                GetDistanceDeviation(patchfacesQuad.ToArray(), faces[0]);

                Snap.NX.Sew sewRectPatchesQuad = Snap.Create.Sew(rectPatchesQuad[0], rectPatchesQuad.GetRange(1, rectPatchesQuad.Count - 1).ToArray());
                GetAngleDeviation(sewRectPatchesQuad.Body.Edges);
                sewRectPatchesQuad.Delete();
                Snap.NX.NXObject.Delete(rectPatchesQuad.ToArray());
                #endregion

                #region Cubic Case

                Snap.InfoWindow.WriteLine("Cubic:   " + facetsCount.ToString());

                List<CubicCurvedTriangle> cubicTriangleList = GetCubicCurvedTriangles(triangleList);

                List<Snap.NX.Bsurface> rectPatchesCubic = new List<Snap.NX.Bsurface>();

                foreach (CubicCurvedTriangle cubicTriangle in cubicTriangleList)
                {
                    Snap.NX.Bsurface rectPatch = converter.GetCubicPinchedRectPatches(cubicTriangle.CubicPoles);
                    rectPatchesCubic.Add(rectPatch);
                }

                List<Snap.NX.Face> patchfacesCubic = new List<Snap.NX.Face>();
                foreach (Snap.NX.Bsurface patch in rectPatchesCubic)
                {
                    patchfacesCubic.Add(patch.Face);
                }
                GetDistanceDeviation(patchfacesCubic.ToArray(), faces[0]);

                Snap.NX.Sew sewRectPatchesCubic = Snap.Create.Sew(rectPatchesCubic[0], rectPatchesCubic.GetRange(1, rectPatchesCubic.Count - 1).ToArray());
                GetAngleDeviation(sewRectPatchesCubic.Body.Edges);
                sewRectPatchesCubic.Delete();
                Snap.NX.NXObject.Delete(rectPatchesCubic.ToArray());
                #endregion
            }
            #endregion

            //Turn on Update
            ufs.Disp.SetDisplay(NXOpen.UF.UFConstants.UF_DISP_UNSUPPRESS_DISPLAY);
            ufs.Disp.RegenerateDisplay();
        }

        private Snap.NX.Face[] GetBezierSphereFaces(Snap.Position center, double radius)
        {
            Snap.NX.Bsurface patchZ = Snap.Create.BezierSpherePatch(center, Snap.Orientation.Identity, radius);
            patchZ.Color = System.Drawing.Color.Red;

            Snap.Orientation orientX = new Snap.Orientation(Snap.Vector.AxisX);
            Snap.NX.Bsurface patchX = Snap.Create.BezierSpherePatch(center, orientX, radius);
            patchX.Color = System.Drawing.Color.Green;

            Snap.Orientation orientY = new Snap.Orientation(Snap.Vector.AxisY);
            Snap.NX.Bsurface patchY = Snap.Create.BezierSpherePatch(center, orientY, radius);
            patchY.Color = System.Drawing.Color.Blue;

            Snap.Geom.Transform mirrorX = Snap.Geom.Transform.CreateReflection(Snap.Vector.AxisX, 0);
            Snap.NX.Body patchXMirror = patchX.Copy(mirrorX);

            Snap.Geom.Transform mirrorY = Snap.Geom.Transform.CreateReflection(Snap.Vector.AxisY, 0);
            Snap.NX.Body patchYMirror = patchY.Copy(mirrorY);

            Snap.Geom.Transform mirrorZ = Snap.Geom.Transform.CreateReflection(Snap.Vector.AxisZ, 0);
            Snap.NX.Body patchZMirror = patchZ.Copy(mirrorZ);


            Snap.NX.Face[] sphereFaces = { patchX.Face, patchXMirror.Faces[0], patchY.Face, patchYMirror.Faces[0], patchZ.Face, patchZMirror.Faces[0] };

            return sphereFaces;
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

        protected override Vertex GetNormsPtsInParamSpace(Snap.NX.Face face,params double[] uv)
        {
            Snap.Position pointOnFace = face.Position(uv);
            Snap.Vector normal = new Snap.Vector(pointOnFace - center);
            Snap.Vector unitNorm = Snap.Vector.Unit(normal);
            return new Vertex(pointOnFace, unitNorm);
        }
    }
}
