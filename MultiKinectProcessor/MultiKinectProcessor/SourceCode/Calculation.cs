using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiKinectProcessor.SourceCode
{
    class Calculation
    {
        /// <summary>
        /// Description: Returns theta (in Radians) given the two location points for a user's center and right shoulder
        /// Original Author: Alex Scarlett
        /// Modified by: Jerry Peng
        /// </summary>
        static public double findUserTheta(double c, double d, double e, double f)
        {
            double A, B, C, preAcos, theta;
            B = System.Math.Sqrt(((c * c) + (d * d)));
            C = System.Math.Sqrt(((e * e) + (f * f)));
            A = System.Math.Sqrt(((c - e) * (c - e) + (d - f) * (d - f)));
            preAcos = ((A * A) + (B * B) - (C * C)) / (2 * A * B);
            if (preAcos <= 1.0 && preAcos >= -1.0)
            {
                theta = System.Math.Acos(preAcos);
            }
            else
            {
                Message.Error("Invalid TRIANGLE, CANNOT COMPUTE THETA");
                return -1;
            }
            return theta;
        }

        /// <summary>
        /// Description: Converts Radians to Degrees
        /// Original Author: Alex Scarlett
        /// </summary>
        static public double radians2Degrees(double radians)
        {
            double degrees = (radians * 180.0) / System.Math.PI;
            return degrees;
        }
    }
}
