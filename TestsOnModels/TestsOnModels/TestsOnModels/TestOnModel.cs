using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PK = PLMComponents.Parasolid.PK_.Unsafe;

namespace TestsOnModels
{
    abstract class TestOnModel
    {
        /// <summary>
        /// Get vertexs on a face, then get triangles from the tessellation of the face
        /// </summary>
        /// <param name="vertexs">Array of vertexs, storing information (points & normals)</param>
        /// <param name="face">The face where tessellation happens</param>
        /// <param name="k"></param>
        abstract protected void GetVertexsOnFace(Vertex[,] vertexs, Snap.NX.Face face, int k);

        abstract protected Vertex GetNormsPtsInParamSpace(Snap.NX.Face face, params double[] uv);

        /// <summary>
        /// Get the list of triangles, triangles are underlying templates for cubic and quadratic curved triangles
        /// </summary>
        /// <param name="vertexs"></param>
        /// <param name="triangleList"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        protected List<Triangle> GetTriangleList(Vertex[,] vertexs, List<Triangle> triangleList, int k)
        {
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    Vertex[] triangleVerts = new Vertex[3] { vertexs[i, j], vertexs[i, j + 1], vertexs[i + 1, j + 1] };
                    Triangle triangle = new Triangle(triangleVerts);
                    triangleList.Add(triangle);
                    triangleVerts = new Vertex[3] { vertexs[i, j], vertexs[i + 1, j], vertexs[i + 1, j + 1] };
                    triangle = new Triangle(triangleVerts);
                    triangleList.Add(triangle);
                }
            }
            return triangleList;
        }

        protected List<QuadCurvedTriangle> GetQuadCurvedTriangles(List<Triangle> triangleList)
        {
            List<QuadCurvedTriangle> quadTriangleList = new List<QuadCurvedTriangle>();
            foreach (Triangle triangle in triangleList)
            {
                QuadCurvedTriangle quadCurvedTriangle = new QuadCurvedTriangle(triangle.Vertexs);
                quadTriangleList.Add(quadCurvedTriangle);
            }
            return quadTriangleList;
        }

        protected List<CubicCurvedTriangle> GetCubicCurvedTriangles(List<Triangle> triangleList)
        {
            List<CubicCurvedTriangle> cubicTriangleList = new List<CubicCurvedTriangle>();
            foreach (Triangle triangle in triangleList)
            {
                CubicCurvedTriangle cubicCurvedTriangle = new CubicCurvedTriangle(triangle.Vertexs);
                cubicTriangleList.Add(cubicCurvedTriangle);
            }
            return cubicTriangleList;
        }

        /// <summary>
        /// Get the maximum angle deviation among the edges
        /// </summary>
        /// <param name="edges"></param>
        protected void GetAngleDeviation(Snap.NX.Edge[] edges)
        {
            double maxAngleError = 0;
            foreach (Snap.NX.Edge edge in edges)
            {
                Snap.NX.Face[] faces = edge.Faces;
                if (faces.Length == 2)
                {
                    // Compute the angular errors between the two neighboring patches.
                    Snap.Compute.DeviationResult dr = Snap.Compute.DeviationInfo(edge, faces[0], faces[1], 20);
                    if (dr.MaximumAngleError > maxAngleError)
                    {
                        maxAngleError = dr.MaximumAngleError;
                    }
                }
            }
            Snap.InfoWindow.WriteLine("The Maximum Angle Error is " + maxAngleError);
        }

        /// <summary>
        /// Get the maximum distance deviation between the original face and the curved face(rectangular patch)
        /// </summary>
        /// <param name="rectFaces"></param>
        /// <param name="originalFace"></param>
        protected void GetDistanceDeviation(Snap.NX.Face[] rectFaces, Snap.NX.Face originalFace)
        {
            double maxDistanceError = 0;
            foreach (Snap.NX.Face patch in rectFaces)
            {
                Snap.Compute.DeviationResult result = Snap.Compute.DeviationInfo(patch, originalFace, 20, 20);
                if (result.MaximumDistanceError > maxDistanceError)
                {
                    maxDistanceError = result.MaximumDistanceError;
                }
            }
            Snap.InfoWindow.WriteLine("The Maximum Distance Error is " + maxDistanceError.ToString());
        }

        //####################################################################
        //####################################################################
        //####################################################################
        //####################################################################
        // The following code for curvature calculation is not invoked in our code
        // BUT we propose using the deviation of Gaussian Curvature as another standard to evaluate our curved triangle technique
        //####################################################################
        //####################################################################
        //####################################################################
        //####################################################################

        #region Functions for Calculating Curvature Deviation between Curved Face and Original Face

        /// <summary>
        /// We are calculating the curvature deviation using two techniques, one is using NX, the other one is using Parasolid
        /// We compared the two techniques, expecting the factor of 1000, because NX unit is Meter while Parasolid uses Millimeter
        /// </summary>
        /// <param name="exactBody">The orignal face</param>
        /// <param name="sheets">The curved face</param>
        /// <param name="deviationList"> Storing the deviation results of Parasolid technique </param>
        /// <param name="_deviationList"> Storing the deviation results of Parasolid technique</param>
        protected void ComputeDeviation(Snap.NX.Face exactBody, Snap.NX.NXObject[] sheets, List<double> deviationList, List<double> _deviationList)
        {
            const int Variation = 4;
            const double Epsilon = 0.01;

            double deltaU, deltaV;

            foreach (Snap.NX.NXObject sheetBody in sheets)
            {
                Snap.NX.Face patch = (Snap.NX.Face)sheetBody;
                Snap.Geom.Box2d box = patch.BoxUV;
                deltaU = (box.MaxU - box.MinU) / Variation;
                deltaV = (box.MaxV - box.MinV) / Variation;
                double u, v;
                int k = 0;
                for (int i = 0; i <= Variation; i++)
                {
                    for (int j = 0; j <= Variation; j++)
                    {
                        u = j * deltaU;
                        v = i * deltaV;
                        Snap.Position ptOnPatch = patch.Position(new double[] { u, v });
                        double gaussianCurvature1 = ComputeGaussianCurvatureParasolid(patch, ptOnPatch);
                        double _gaussianCurvature1 = ComputeGaussianCurvatureNX(patch, new double[] { u, v });
                        Snap.InfoWindow.WriteLine("The " + k.ToString() + "th Check Point");
                        Snap.InfoWindow.WriteLine("NX Gaussian Curvature On Curved Patch: " + gaussianCurvature1.ToString());
                        Snap.InfoWindow.WriteLine("Parasolid Gaussian Curvature On Curved Patch: " + _gaussianCurvature1.ToString());
                        double[] uv = exactBody.Parameters(ptOnPatch);
                        Snap.Position ptOnExactFace = exactBody.Position(uv);
                        //Snap.NX.Sphere sphere2 = Snap.Create.Sphere(ptOnExactFace, 1);
                        //sphere2.Color = System.Drawing.Color.Green;
                        double gaussianCurvature2 = ComputeGaussianCurvatureParasolid(exactBody, ptOnExactFace);
                        double _gaussianCurvature2 = ComputeGaussianCurvatureNX(exactBody, uv);
                        Snap.InfoWindow.WriteLine("NX Gaussian Curvature On Original Object: " + gaussianCurvature2.ToString());
                        Snap.InfoWindow.WriteLine("Parasolid Gaussian Curvature On Original Object: " + _gaussianCurvature2.ToString());
                        double deviation = System.Math.Abs(gaussianCurvature1 - gaussianCurvature2) / (Epsilon + System.Math.Abs(gaussianCurvature2));
                        double _deviation = System.Math.Abs(_gaussianCurvature1 - _gaussianCurvature2) / (Epsilon + System.Math.Abs(_gaussianCurvature2));
                        deviationList.Add(deviation);
                        _deviationList.Add(_deviation);
                        k++;
                        Snap.InfoWindow.WriteLine("\n");
                    }
                }
            }
        }

        /// <summary>
        /// Compute Gaussian Curvature using Parasolid methods, which give answers in meters
        /// </summary>
        /// <param name="face"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private double ComputeGaussianCurvatureParasolid(Snap.NX.Face face, Snap.Position point)
        {
            PK.FACE_t psFace = GetPKFace(face.NXOpenTag);
            PK.SURF_t surf;
            unsafe
            {
                PK.FACE.ask_surf(psFace, &surf);
                PK.VECTOR_t position = new PK.VECTOR_t(point.X, point.Y, point.Z);
                PK.UV_t uv;
                try
                {
                    PK.ERROR.code_t error = PK.SURF.parameterise_vector(surf, position, &uv);
                }
                catch (System.Runtime.InteropServices.SEHException e)
                {
                    //Snap.InfoWindow.WriteLine(e.Message.ToString());
                }
                PK.VECTOR1_t n, d1, d2;
                double k1, k2;

                PK.SURF.eval_curvature(surf, uv, &n, &d1, &d2, &k1, &k2);
                double gaussianCurvature = k1 * k2;
                return gaussianCurvature;
            }
        }

        /// <summary>
        /// Compute Gaussian Curvature using NX methods, which give answers in millimeters
        /// </summary>
        /// <param name="face"></param>
        /// <param name="uv"></param>
        /// <returns></returns>
        private double ComputeGaussianCurvatureNX(Snap.NX.Face face, params double[] uv)
        {
            NXOpen.UF.UFSession ufs = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.ModlSrfValue surfaceValues = new NXOpen.UF.ModlSrfValue();
            int mode = NXOpen.UF.UFConstants.UF_MODL_EVAL_DERIV2;
            ufs.Modl.EvaluateFace(face.NXOpenTag, mode, uv, out surfaceValues);

            Snap.Vector Su = surfaceValues.srf_du;
            Snap.Vector Sv = surfaceValues.srf_dv;
            Snap.Vector Suu = surfaceValues.srf_d2u;
            Snap.Vector Suv = surfaceValues.srf_dudv;
            Snap.Vector Svv = surfaceValues.srf_d2v;

            Snap.Vector C = Snap.Vector.Cross(Su, Sv);

            double numer = (C * Suu) * (C * Svv) - (C * Suv) * (C * Suv);
            double denom = (C * C) * (C * C);

            return numer / denom;
        }

        private PK.FACE_t GetPKFace(NXOpen.Tag NXOpenTag)
        {
            NXOpen.Tag psTag;
            NXOpen.UF.UFSession theUfs = NXOpen.UF.UFSession.GetUFSession();
            theUfs.Ps.AskPsTagOfObject(NXOpenTag, out psTag);
            PK.FACE_t psFace = (PK.FACE_t)(int)psTag;
            return psFace;
        }
        
        #endregion
    }
}
