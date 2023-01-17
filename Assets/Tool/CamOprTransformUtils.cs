using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CamOpr.Tool
{
	static class CamOprTransformUtils
	{
		/// <summary>
		/// クォータニオンの対数を取得する(接空間を表す)
		/// Get the logarithm of Quaternion (representing the tangent space)
		/// </summary>
		/// <param name="quaternion">クォータニオン</param>
		public static Vector3 LogQuaternion(Quaternion quaternion)
		{
			float a, isina;

			if (quaternion.w > 1.0 - 1.0e-6)
			{
				a = 0.0f;
				isina = 0.0f;
			}
			else
			{
				a = Mathf.Acos(quaternion.w);
				isina = a / Mathf.Sin(a);
			}

			return new Vector3(quaternion.x * isina, quaternion.y * isina, quaternion.z * isina);
		}

		/// <summary>
		/// 対数クォータニオンをクォータニオンに変換する
		/// Convert a logarithmic quaternion to a quaternion
		/// </summary>
		/// <param name="logQuaternion">対数クォータニオン</param>
		public static Quaternion ConvertLogQuaternion(Vector3 logQuaternion)
		{
			float sina;
			float mag = Mathf.Sqrt(Mathf.Pow(logQuaternion.x, 2) + Mathf.Pow(logQuaternion.y, 2) + Mathf.Pow(logQuaternion.z, 2));

			if (Mathf.Abs(mag) < 1.0e-6)
			{
				sina = 0;
			}
			else
			{
				sina = Mathf.Sin(mag) / mag;
			}

			return new Quaternion(logQuaternion.x * sina,
								  logQuaternion.y * sina,
								  logQuaternion.z * sina,
								  Mathf.Cos(mag));
		}
	}
}
