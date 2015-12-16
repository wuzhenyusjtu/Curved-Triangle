using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snap;

namespace CurvedTriangle
{
    class MathFunctions
    {
        ///((Ps-Pt)*e) = cos(halfangle)*|Ps-Pt|*|e|, where Pt is the tip of the cone, e is the axis vector of the cone
        /// <summary>solve the eqution: (x*a+b)*c = m*|x*a+b|, where a,b,c are vectors, m is a scalar.</summary>
        /// <param name="a">Pa - Pm</param>
        /// <param name="b"> Pm - Pt</param>
        /// <param name="c"> axis vector of the cone</param>
        /// <param name="m"> cos(halfAngle)</param>
        /// <returns> double value </returns>
        public static double solveQuadraticEqt(Vector a, Vector b, Vector c, double m)
        {
            double a0 = System.Math.Pow(a * c, 2) - System.Math.Pow(m, 2) * (a * a);
            double a1 = ((a * c) * (b * c) - System.Math.Pow(m, 2) * a * b) * 2;
            double a2 = System.Math.Pow(b * c, 2) - System.Math.Pow(m, 2) * (b * b);

            double[] roots;
            // a0 = 0, then it's a linear equation
            if (System.Math.Abs(a0) < 1e-12)
            {
                roots = new double[] { -a2 / a1 }; // Real roots
                if (roots[0] >= 0 && roots[0] <= 1)
                {
                    return roots[0];
                }
                else return 0;
            }
            // a0 != 0, quadratic equadtion
            else
            {
                double delta = a1 * a1 - 4 * a0 * a2;
                if (delta >= 0)
                {
                    double q = -(a1 + System.Math.Sign(a1) * System.Math.Sqrt(delta)) / 2; // To avoid cancellation error
                    double x1 = q / a0;
                    double x2 = a2 / q;
                    roots = new double[] { x1, x2 }; // Real roots
                    if (roots[0] >= 0 && roots[0] <= 1)
                    {
                        return roots[0];
                    }
                    else if (roots[1] >= 0 && roots[1] <= 1)
                    {
                        return roots[1];
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
