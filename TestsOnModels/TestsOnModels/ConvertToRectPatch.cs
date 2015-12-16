using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace TestsOnModels
{
    /// <summary>
    /// Class includes the functions that build Bezier rectangular patches
    /// </summary>
    public class ConvertToRectangularPatch
    {
        public ConvertToRectangularPatch() { }

        #region Convert to Cubic Rectangular Pactch

        #region Cubic Pinched Technique
        /// <summary>
        /// Convert a cubic triangular patch into a rectangular pinched one, then put it into NX
        /// </summary>
        /// <param name="cubicTrianglePoles">The ten control points of the curved triangle</param>
        /// <returns>The Bsurface of the rectangular patch</returns>
        /// <remarks>
        /// The output is a rectangular patch that has one side "pinched" (shrunk to a point).
        /// </remarks>
        public Snap.NX.Bsurface GetCubicPinchedRectPatches(CubicTrianglePoles cubicPoles)
        {
            Position[,] rectPoints = GetPinchedRectPoints(cubicPoles);

            // Create Bezier patch in NX
            Snap.NX.Bsurface bs = Snap.Create.BezierPatch(rectPoints);
            return bs;
        }

        /// <summary>
        /// Get the sixteen points for the rectangular bicubic patch
        /// </summary>
        /// <param name="cubicTrianglePoles">The ten control points of the curved cubic triangle</param>
        /// <returns></returns>
        private Position[,] GetPinchedRectPoints(CubicTrianglePoles cubicPoles)
        {
            Position[,] rectPoints = new Position[4, 4];

            // Need documentation of array of ten points in cubic triangular patch

            rectPoints[0, 0] = cubicPoles.B030;
            rectPoints[0, 1] = cubicPoles.B120;
            rectPoints[0, 2] = cubicPoles.B210;
            rectPoints[0, 3] = cubicPoles.B300;

            rectPoints[1, 0] = cubicPoles.B021;
            rectPoints[1, 1] = (cubicPoles.B120 + 2 * cubicPoles.B111) / 3;
            rectPoints[1, 2] = (2 * cubicPoles.B210 + cubicPoles.B201) / 3;
            rectPoints[1, 3] = cubicPoles.B300;

            rectPoints[2, 0] = cubicPoles.B012;
            rectPoints[2, 1] = (2 * cubicPoles.B111 + cubicPoles.B102) / 3;
            rectPoints[2, 2] = (cubicPoles.B210 + 2 * cubicPoles.B201) / 3;
            rectPoints[2, 3] = cubicPoles.B300;

            rectPoints[3, 0] = cubicPoles.B003;
            rectPoints[3, 1] = cubicPoles.B102;
            rectPoints[3, 2] = cubicPoles.B201;
            rectPoints[3, 3] = cubicPoles.B300;

            return rectPoints;
        }
        #endregion

        #region Cubic Packed Technique
        /// <summary>
        /// Convert a cubic triangular patch into three rectangles packed one, then put them into NX
        /// </summary>
        /// <param name="cubicTrianglePoles">The the control points of the curved triangle</param>
        /// <returns>The Bsurfaces of three rectangular patches</returns>
        /// <remarks>
        /// The output is three rectangular patches that packed together.
        /// </remarks>
        public Snap.NX.Bsurface[] GetCubicPackedRectPatches(CubicTrianglePoles cubicPoles)
        {
            CubicTrianglePoles tri1, tri2, tri3;
            GetThreeTriPoles(cubicPoles, out tri1, out tri2, out tri3);
            Snap.NX.Bsurface[] rectPatches = GetPackedRectPatches(tri1, tri2, tri3);
            return rectPatches;
        }

        /// <summary>
        /// Re-order the ten control points of the cubic triangular patch in three different ways
        /// </summary>
        /// <param name="cubicTrianglePoles">Ten control points of the cubic curved triangle</param>
        /// <param name="tri1">Trianglle poles re-ordered one way</param>
        /// <param name="tri2">Re-ordered second way</param>
        /// <param name="tri3">Re-ordered third way</param>
        private void GetThreeTriPoles(CubicTrianglePoles cubicPoles, out CubicTrianglePoles tri1, out CubicTrianglePoles tri2, out CubicTrianglePoles tri3)
        {
            Position b30 = cubicPoles.B300;
            Position b21 = cubicPoles.B210;
            Position b12 = cubicPoles.B120;
            Position b03 = cubicPoles.B030;
            Position b02 = cubicPoles.B021;
            Position b01 = cubicPoles.B012;
            Position b00 = cubicPoles.B003;
            Position b10 = cubicPoles.B102;
            Position b20 = cubicPoles.B201;
            Position b11 = cubicPoles.B111;

            // Change the order of the ten poles, so that we can get three rectangles.
            Position[] tri1Array = new Position[10] { b30, b20, b10, b00, b01, b02, b03, b12, b21, b11 };
            tri1 = new CubicTrianglePoles(tri1Array);
            Position[] tri2Array = new Position[10] { b03, b12, b21, b30, b20, b10, b00, b01, b02, b11 };
            tri2 = new CubicTrianglePoles(tri2Array);
            Position[] tri3Array = new Position[10] { b00, b01, b02, b03, b12, b21, b30, b20, b10, b11 };
            tri3 = new CubicTrianglePoles(tri3Array);
        }

        /// <summary>
        /// Create Bezier patches in NX
        /// </summary>
        /// <param name="rect1"></param>
        /// <param name="rect2"></param>
        /// <param name="rect3"></param>
        /// <returns></returns>
        private Snap.NX.Bsurface[] GetPackedRectPatches(CubicTrianglePoles rect1, CubicTrianglePoles rect2, CubicTrianglePoles rect3)
        {
            Snap.NX.Bsurface[] rectPatches = new Snap.NX.Bsurface[3];

            rectPatches[0] = Snap.Create.BezierPatch(GetPackedRectPoints(rect1));
            rectPatches[1] = Snap.Create.BezierPatch(GetPackedRectPoints(rect2));
            rectPatches[2] = Snap.Create.BezierPatch(GetPackedRectPoints(rect3));

            return rectPatches;
        }

        /// <summary>Get the sixteen points of one of the three packed rectangular patches</summary>
        /// <param name="cubicTrianglePoles"> Array of the ten poles of the cubic triangle </param>
        /// <returns>Sixteen points of one of the three rectangles</returns>
        private Position[,] GetPackedRectPoints(CubicTrianglePoles cubicPoles)
        {

            Position b00 = cubicPoles.B300;
            Position b01 = cubicPoles.B210;
            Position b02 = cubicPoles.B120;
            Position b03 = cubicPoles.B030;
            Position b12 = cubicPoles.B021;
            Position b21 = cubicPoles.B012;
            Position b30 = cubicPoles.B003;
            Position b20 = cubicPoles.B102;
            Position b10 = cubicPoles.B201;
            Position b11 = cubicPoles.B111;

            CubicRectanglePoles poles = new CubicRectanglePoles();

            // Formulas from Shi-Min Hu paper
            poles.B00 = b00;
            poles.B01 = (b01 + b00) / 2;
            poles.B02 = (b02 + 2 * b01 + b00) / 4;
            poles.B03 = (b03 + 3 * b02 + 3 * b01 + b00) / 8;
            poles.B10 = (b10 + b00) / 2;
            poles.B11 = (3 * b11 + 5 * b10 + 5 * b01 + 5 * b00) / 18;
            poles.B12 = (3 * b12 + 14 * b11 + 11 * b10 + 11 * b02 + 22 * b01 + 11 * b00) / 72;
            poles.B13 = (b12 + 2 * b11 + b10 + b03 + 3 * b02 + 3 * b01 + b00) / 12;
            poles.B20 = (b20 + 2 * b10 + b00) / 4;
            poles.B21 = (3 * b21 + 11 * b20 + 14 * b11 + 22 * b10 + 11 * b01 + 11 * b00) / 72;
            poles.B22 = (3 * b21 + 5 * b20 + 3 * b12 + 13 * b11 + 10 * b10 + 5 * b02 + 10 * b01 + 5 * b00) / 54;
            poles.B23 = (b21 + b20 + 2 * b12 + 4 * b11 + 2 * b10 + b03 + 3 * b02 + 3 * b01 + b00) / 18;
            poles.B30 = (b30 + 3 * b20 + 3 * b10 + b00) / 8;
            poles.B31 = (b30 + b21 + 3 * b20 + 2 * b11 + 3 * b10 + b01 + b00) / 12;
            poles.B32 = (b30 + 2 * b21 + 3 * b20 + b12 + 4 * b11 + 3 * b10 + b02 + 2 * b01 + b00) / 18;
            poles.B33 = (b30 + 3 * b21 + 3 * b20 + 3 * b12 + 6 * b11 + 3 * b10 + b03 + 3 * b02 + 3 * b01 + b00) / 27;

            return poles.CubicRectPoles;

        }
        #endregion

        #endregion

        #region Convert to Quadratic Rectangular Patch

        #region Quadratic Pinched Technique
        /// <summary> convert the quadratic triangle into the pinched rectangular one, then put it into NX </summary>
        /// <param name="sixPoles"> the poles of the quadratic triangle</param>
        /// <param name="sixWeights"> the weights of the quadratic triangle</param>
        /// <remarks>
        /// The output is three rectangular patches that pinched together.
        /// </remarks>
        public Snap.NX.Bsurface GetQuadraticPinchedRectPatch(QuadTrianglePoles quadTriPoles)
        {
            QuadRectanglePoles quadRectPoles = GetQuadraticPinchedRectPoints(quadTriPoles);
            Snap.NX.Bsurface rectPatch = Snap.Create.BezierPatch(quadRectPoles.QuadRectPoints, quadRectPoles.QuadRectWeights);
            return rectPatch;
        }

        /// <summary>
        /// Get the points of the rectangular patch
        /// </summary>
        /// <param name="sixPoles"> the poles of the quadratic triangle</param>
        /// <param name="sixWeights"> the weights of the quadratic triangle</param>
        /// <param name="rectPoles"> the poles of the pinched rectangular one</param>
        /// <param name="rectWeights"> the weights of the pinched rectangular one </param>
        private QuadRectanglePoles GetQuadraticPinchedRectPoints(QuadTrianglePoles quadTriPoles)
        {
            QuadRectanglePoles quadRectPoles = new QuadRectanglePoles();

            quadRectPoles.P00 = quadTriPoles.P00;
            quadRectPoles.P01 = quadTriPoles.P01;
            quadRectPoles.P02 = quadTriPoles.P02;
            quadRectPoles.P10 = quadTriPoles.P10;
            quadRectPoles.P11 = (quadTriPoles.W10 * quadTriPoles.P10 + quadTriPoles.W11 * quadTriPoles.P11) / (quadTriPoles.W10 + quadTriPoles.W11);
            quadRectPoles.P12 = quadTriPoles.P11;
            quadRectPoles.P20 = quadTriPoles.P20;
            quadRectPoles.P21 = quadTriPoles.P20;
            quadRectPoles.P22 = quadTriPoles.P20;

            quadRectPoles.W00 = 1;
            quadRectPoles.W01 = quadTriPoles.W01;
            quadRectPoles.W02 = 1;
            quadRectPoles.W10 = quadTriPoles.W10;
            quadRectPoles.W11 = (quadTriPoles.W10 + quadTriPoles.W11) / 2;
            quadRectPoles.W12 = quadTriPoles.W11;
            quadRectPoles.W20 = 1;
            quadRectPoles.W21 = 1;
            quadRectPoles.W22 = 1;

            return quadRectPoles;
        }
        #endregion

        #region Quadratic Packed Technique

        // Not yet implemented

        #endregion

        #endregion
    }
}
