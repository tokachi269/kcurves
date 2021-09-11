using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
	 static class QuaternionEx
	{
		public static void Log(ref Quaternion a)
		{
			float a0 = a.w;
			a.w = 0f;

			//Absで絶対値を取得
			if (Mathf.Abs(a0) < 1.0)
			{
				//オイラー角を計算
				float angle = Mathf.Acos(a0); 
				

				float sinAngle = Mathf.Sin(angle);
				if (Mathf.Abs(sinAngle) >= 1.0e-15)
				{
					float coeff = angle / sinAngle;
					a.x *= coeff;
					a.y *= coeff;
					a.z *= coeff;
				}
			}
		}

		public static void Scale(ref Quaternion a, float s)
		{
			a.w *= s;
			a.x *= s;
			a.y *= s;
			a.z *= s;
		}

		public static Quaternion Add(Quaternion a, Quaternion b)
		{
			float x = a.x + b.x;
			float y = a.y + b.y;
			float z = a.z + b.z;
			float w = a.w + b.w;
			return new Quaternion(x,y,z,w);
		}

		public static void Exp(ref Quaternion a)
		{
			float angle = Mathf.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
			float sinAngle = Mathf.Sin(angle);
			a.w = Mathf.Cos(angle);
			if (Mathf.Abs(sinAngle) >= 1.0e-15)
			{
				float coeff = sinAngle / angle;
				a.x *= coeff;
				a.y *= coeff;
				a.z *= coeff;

			}

		}

		public static Quaternion Exped(Quaternion a) {
			Quaternion result = a;
			float angle = Mathf.Sqrt(result.x * result.x + result.y * result.y + result.z * result.z);
			float sinAngle  = Mathf.Sin(angle);
			result.w = Mathf.Cos(angle);
			if (Mathf.Abs(sinAngle) >= 1.0e-15) {
				float coeff = sinAngle / angle;
				result.x *= coeff;
				result.y *= coeff;
				result.z *= coeff;
					}
			return result;
			}


		public static Quaternion SlerpNoInvert(Quaternion from , Quaternion  to ,float t)
        {
			//内積を計算
			float dot  = Quaternion.Dot(from, to);

			if (Mathf.Abs(dot) > 0.9999f) return from;

			//角度を求める
			float theta = Mathf.Acos(dot);

			float sinT = 1.0f / Mathf.Sin(theta);
			float newFactor = Mathf.Sin(t * theta) * sinT;
			float invFactor = Mathf.Sin((1.0f - t) * theta) * sinT;

			Quaternion q;
			q.x = invFactor * from.x + newFactor * to.x;
			q.y = invFactor * from.y + newFactor * to.y;
			q.z = invFactor * from.z + newFactor * to.z;
			q.w = invFactor * from.w + newFactor * to.w;

			return q;
		}

	}
}
