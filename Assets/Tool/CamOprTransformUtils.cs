using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CamOpr.Tool
{
    static class CamOprTransformUtils
    {
		/// <summary>
		/// 補完されたクォータニオンを返す
		/// Return the completed quaternion
		/// </summary>
		/// <param name="knots">リスト</param>
		/// <param name="knotIndex">インデックス</param>
		/// <param name="t">パラメータ</param>
		public static Quaternion SplineQuaternion(ref List<CameraConfig> knots ,int knotIndex, float t )
        {
			int i = knotIndex;

			if (i == 0)
			{
				Debug.Log("" + i + ", " + i + ", " + (i + 1) + ", " + (i + 2) + ", ");
				return ConvertLogQuaternion(
					GetCatmullRomPosition(LogQuaternion(knots[i    ].Rotation),
										  LogQuaternion(knots[i    ].Rotation),
										  LogQuaternion(knots[i + 1].Rotation),
										  LogQuaternion(knots[i + 2].Rotation), t));
			}
			else if (i == knots.Count()- 2 && i > 0){
				Debug.Log("" + (i - 1)+", " + i + ", " + (i + 1) + ", " + (i + 1) + ", ");
				return ConvertLogQuaternion(
					GetCatmullRomPosition(LogQuaternion(knots[i - 1].Rotation),
										  LogQuaternion(knots[i    ].Rotation),
										  LogQuaternion(knots[i + 1].Rotation),
										  LogQuaternion(knots[i + 1].Rotation), t));
			}
			else if (i >= 1 && i < knots.Count() - 2){
				Debug.Log("" + (i-1) + ", " + i + ", " + (i + 1) + ", " + (i + 2) + ", ");
				return ConvertLogQuaternion(
					GetCatmullRomPosition(LogQuaternion(knots[i - 1].Rotation),
										  LogQuaternion(knots[i    ].Rotation),
										  LogQuaternion(knots[i + 1].Rotation),
										  LogQuaternion(knots[i + 2].Rotation), t));
			}
			return Quaternion.identity;
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
		private static Vector3 GetCatmullRomPosition( Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float t)
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
