using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CamOpr.Tool
{
    static class BezierUtils
    {
		/// <summary>
		/// 補完されたクォータニオンを返す
		/// Return the completed quaternion
		/// </summary>
		/// <param name="knots">リスト</param>
		/// <param name="knotIndex">インデックス</param>
		/// <param name="t">パラメータ</param>
		/// <returns></returns>
		public static object Spline<T>(float t, T p1, T p2, T p3, T p4)
		{
			Debug.Log("BezierUtils.Spline<T> float t: " + t);

			Type fieldType = typeof(T);

			if (fieldType.Equals(typeof(Quaternion)))
			{
				return CamOprTransformUtils.ConvertLogQuaternion(
					GetCatmullRom(
						CamOprTransformUtils.LogQuaternion((Quaternion)(object)p1),
						CamOprTransformUtils.LogQuaternion((Quaternion)(object)p2),
						CamOprTransformUtils.LogQuaternion((Quaternion)(object)p3),
						CamOprTransformUtils.LogQuaternion((Quaternion)(object)p4),
						t));
			}

			if (fieldType.Equals(typeof(Vector3)))
			{
				return GetCatmullRom(
						(Vector3)(object)p1,
						(Vector3)(object)p2,
						(Vector3)(object)p3,
						(Vector3)(object)p4,
						t);
			}

			if (fieldType.Equals(typeof(Vector2)))
			{
				return GetCatmullRom(
						(Vector2)(object)p1,
						(Vector2)(object)p2,
						(Vector2)(object)p3,
						(Vector2)(object)p4,
						t);
			}

			if (fieldType.Equals(typeof(float)))
			{
				return GetCatmullRom(
						(float)(object)p1,
						(float)(object)p2,
						(float)(object)p3,
						(float)(object)p4,
						t);
			}

			throw new InvalidCastException("An unexpected type was passed." + fieldType);
		}

		/// <summary>
		/// 補完されたクォータニオンを返す
		/// Return the completed quaternion
		/// </summary>
		/// <param name="knots">リスト</param>
		/// <param name="knotIndex">インデックス</param>
		/// <param name="t">パラメータ</param>
		/// <returns></returns>
		public static object SplineKnots<T>(ref List<CameraConfig> knots, string fieldName, int i, float t)
		{
			FieldInfo fieldInfo = typeof(CameraConfig).GetField(fieldName);
			Type fieldType = typeof(CameraConfig).GetField(fieldName).FieldType;

			int[] array = GetSplineIndex(knots.Count(), i);

            return Spline<T>(t, (T)fieldInfo.GetValue(knots[array[0]]), (T)fieldInfo.GetValue(knots[array[1]]), (T)fieldInfo.GetValue(knots[array[2]]), (T)fieldInfo.GetValue(knots[array[3]]));
		}

		/// <summary>
		/// 補完されたクォータニオンを返す
		/// Return the completed quaternion
		/// </summary>
		/// <param name="knots">リスト</param>
		/// <param name="knotIndex">インデックス</param>
		/// <param name="t">パラメータ</param>
		/// <returns></returns>
		public static Quaternion SplineQuaternion(ref List<CameraConfig> knots, int i, float t)
		{
			int[] array = GetSplineIndex(knots.Count(), i);

			//Debug.Log("" + i + ", " + i + ", " + (i + 1) + ", " + (i + 2) + ", ");
			return CamOprTransformUtils.ConvertLogQuaternion(
				GetCatmullRom(
					CamOprTransformUtils.LogQuaternion(knots[array[0]].Rotation),
					CamOprTransformUtils.LogQuaternion(knots[array[1]].Rotation),
					CamOprTransformUtils.LogQuaternion(knots[array[2]].Rotation),
					CamOprTransformUtils.LogQuaternion(knots[array[3]].Rotation),
					t));
		}

		public static int[] GetSplineIndex(int count, int i)
		{
			int[] array = new int[4];

			if (i == 0)
			{
				array[0] = i;
				array[1] = i;
				array[2] = i + 1;
				array[3] = i + 2;
			}
			else if (i == count - 2 && i > 0)
			{
				array[0] = i - 1;
				array[1] = i;
				array[2] = i + 1;
				array[3] = i + 1;
			}
			else if (i >= 1 && i < count - 2)
			{
				array[0] = i - 1;
				array[1] = i;
				array[2] = i + 1;
				array[3] = i + 2;
			}

			return array;
		}

		/// <summary>
		/// CatmullRomを使用した補完を取得する
		/// Get completion using CatmullRom
		/// </summary>
		/// <param name="v0">座標0</param>
		/// <param name="v1">座標1</param>
		/// <param name="v2">座標2</param>
		/// <param name="v3">座標3</param>
		/// <param name="t">パラメータ</param>
		private static Vector3 GetCatmullRom(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float t)
		{
			//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
			Vector3 a = 2f * v1;
			Vector3 b = v2 - v0;
			Vector3 c = 2f * v0 - 5f * v1 + 4f * v2 - v3;
			Vector3 d = -v0 + 3f * v1 - 3f * v2 + v3;

			//The cubic polynomial: a + b * t + c * t^2 + d * t^3
			Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

			return pos;
		}

		/// <summary>w
		/// CatmullRomを使用した補完を取得する
		/// Get completion using CatmullRom
		/// </summary>
		/// <param name="v0">座標0</param>
		/// <param name="v1">座標1</param>
		/// <param name="v2">座標2</param>
		/// <param name="v3">座標3</param>
		/// <param name="t">パラメータ</param>
		private static float GetCatmullRom(float v0, float v1, float v2, float v3, float t)
		{

			//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
			float a = 2f * v1;
			float b = v2 - v0;
			float c = 2f * v0 - 5f * v1 + 4f * v2 - v3;
			float d = -v0 + 3f * v1 - 3f * v2 + v3;

			//The cubic polynomial: a + b * t + c * t^2 + d * t^3
			float pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

			return pos;
		}

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
