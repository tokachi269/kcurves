using System;

using UnityEngine;

namespace Assets
{
    static class BezierUtil
    {
        /** tの2次ベジェ曲線の座標を求める */
        public static Vector3 Position(Vector3 P1, Vector3 P2, Vector3 P3, float t)
        {
            return (1 - t) * (1 - t) * P1 + 2 * (1 - t) * t * P2 + t * t * P3;
        }

        public static Vector3 Position(Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * P1 +
                    3f * oneMinusT * oneMinusT * t * P2 +
                    3f * oneMinusT * t * t * P3 +
                    t * t * t * P4;
        }

        /** 2次ベジェ曲線の弧長を媒介変数tで積分して求める */
        public static float CalcLength(Vector3 P1, Vector3 P2, Vector3 P3, double t)
        {
            //TODO P3が Vector3.zeroのときNaNになる
            // ASSERT:  P1, P2, and P3 are distinct points.    
            // The position is the following vector-valued function for 0 <= t <= 1.    
            //   P(t) = (1-t)^2*P1 + 2*(1-t)*t*P2 + t^2*P3.    
            // The derivative is    
            //   P'(t) = (-2*(1-t)*P1) + (2*(1-2*t)*P2) + (2*t*P3)    
            //         = 2*(P1 - 2*P2 + P3)*t + 2*(P2 - P1)   
            //         = 2*A[1]*t + 2*A[0]    
            // The squared length of the derivative is    
            //   f(t) = 4*Dot(A[1],A[1])*t^2 + 8*Dot(A[0],A[1])*t + 4*Dot(A[0],A[0])    

            double length;

            // A[0] not zero by assumption
            Vector3[] A = { P2 - P1,
                            2 * P1 - 2.0f * P2 + P3 };

            if (A[1] != Vector3.zero)
            {
                // Coefficients of f(t) = c*t^2 + b*t + a
                double c = 4.0f * Vector3.Dot(A[1], A[1]);
                // c > 0 to be in this block of code
                double b = 8.0f * Vector3.Dot(A[0], A[1]);
                double a = 4.0f * Vector3.Dot(A[0], A[0]);
                // a > 0 by assumption        
                double q = 4.0f * a * c - b * b;
                // = 16*|Cross(A0,A1)| >= 0        
                // Antiderivative of sqrt(c*t^2 + b*t + a) is
                // F(t) = (2*c*t + b)*sqrt(c*t^2 + b*t + a)/(4*c)        
                //   + (q/(8*c^{3/2}))*log(2*sqrt(c*(c*t^2 + b*t + a)) + 2*c*t + b)        
                // Integral is F(1) - F(0).
                if (t == 0)
                {
                    double twoCpB = 2.0f * c + b;
                    length = ((0.25f / c) * (twoCpB * Math.Sqrt(c + b + a) - b * Math.Sqrt(a))
                             + (q / (8.0f * c * (Math.Sqrt(c)))) * (Math.Log(2.0f * Math.Sqrt(c * c + b + a) + twoCpB) - Math.Log(2.0f * Math.Sqrt(c * a) + b)));
                }
                else
                {
                    length = (2d * c * t + b) * Math.Sqrt(c * t * t + b * t + a) / (4d * c)
                            + (q / (8d * Math.Pow(c, 1.5d))) * Math.Log(2d * Math.Sqrt(c * (c * t * t + b * t + a)) + 2d * c * t + b);
                }
            }
            else
            {
                length = 2.0f * A[0].magnitude;
            }
            return (float)length;
        }

        public static Vector3[] Divide(Vector3 P1, Vector3 P2, Vector3 P3, float t)
        {
            Vector3[] Points = { P1,
                                 (1 - t) * P1 + t * P2 ,
                                 (1 - t) * (1 - t) * P1 + 2 * (1 - t) * t * P2 + t *t * P3 ,
                                 (1 - t) * P2 + t * P3 ,
                                 P3
                               };
            return Points;
        }
    }
}
