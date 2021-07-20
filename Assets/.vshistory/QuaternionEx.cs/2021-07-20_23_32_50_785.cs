using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
	 static class QuaternionEx
	{
		public void Log(ref Quaternion a)
		{
			float a0 = a.w;
			a.w = 0f;
			if (Mathf.Abs(a0) < 1.0)
			{
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

		public Quaternion Loged(ref Quaternion a)
		{
			Quaternion result = a;
			float a0 = result.w;
			result.w = 0f;
			if (Mathf.Abs(a0) < 1.0)
			{
				float angle = Mathf.Acos(a0);
				float sinAngle = Mathf.Sin(angle);
				if (Mathf.Abs(sinAngle) >= 1.0e-15)
				{
					float coeff = angle / sinAngle;
					result.x *= coeff;
					result.y *= coeff;
					result.z *= coeff;
				}
			}

			return result;
		}

		public void Conjugate(Quaternion a)
		{
			a.x *= -1;
			a.y *= -1;
			a.z *= -1;
		}

		public Quaternion Conjugated(Quaternion a)
		{
			Quaternion result = a;
			result.x *= -1;
			result.y *= -1;
			result.z *= -1;
			return result;

		}


		public void Scale(ref Quaternion a, float s)
		{
			a.w *= s;
			a.x *= s;
			a.y *= s;
			a.z *= s;
		}

		public Quaternion Scaled(Quaternion a, float s)
		{
			Quaternion result = a;
			result.w *= s;
			result.x *= s;
			result.y *= s;
			result.z *= s;
			return result;

		}

		public void Exp(ref Quaternion a)
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

		public Quaternion Exped(Quaternion a) {
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

		//public float Normalize(ref Quaternion a)
		//{
		//	float length = a.Length();
		//	if (length > 1.0e-15)
		//	{
		//		float invlen = 1.0f / length;
		//		a.w *= invlen;
		//		a.x *= invlen;
		//		a.y *= invlen;
		//		a.z *= invlen;
		//	}
		//	else
		//	{
		//		length = 0f;
		//		a.w = 0f;
		//		a.x = 0f;
		//		a.y = 0f;
		//		a.z = 0f;
		//	}
		//	return length;
		//}

		public Quaternion SlerpNoInvert(Quaternion fro , Quaternion  to ,float factor)
        {
			float dot  = Quaternion.Dot(fro, to);

			if (Mathf.Abs(dot) > 0.9999f) return fro;

			float theta = Mathf.Acos(dot);
			float sinT = 1.0f / Mathf.Sin(theta);
			float newFactor = Mathf.Sin(factor * theta) * sinT;
			float invFactor = Mathf.Sin((1.0f - factor) * theta) * sinT;

			Quaternion q;
			q.x = invFactor * fro.x + newFactor * to.x;
			q.y = invFactor * fro.y + newFactor * to.y;
			q.z = invFactor * fro.z + newFactor * to.z;
			q.w = invFactor * fro.w + newFactor * to.w;

			return q;
		}

	}
}
