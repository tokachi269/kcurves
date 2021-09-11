﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    static class Squad
    {
		// Returns a smoothed quaternion along the set of quaternions making up the spline, each quaternion is along an equidistant value in t
		public static Quaternion Spline(ref List<CameraConfig> knots ,int knotIndex, int count,float  t )
        {
			int i = knotIndex;

			if (i == 0)
			{
				return SplineSegment(knots[i].rotation, knots[i].rotation, knots[i + 1].rotation, knots[i + 2].rotation, t);
			}
			else if (i == count - 2 && i > 0){
				return SplineSegment(knots[i - 1].rotation, knots[i].rotation, knots[i + 1].rotation, knots[i + 1].rotation, t);
			}
			else if (i >= 1 && i < count - 2){
				return SplineSegment(knots[i - 1].rotation, knots[i].rotation, knots[i + 1].rotation, knots[i + 2].rotation, t);
			}
			return Quaternion.identity;
		}

		// Returns a quaternion between q1 and q2 as part of a smooth SQUAD segment
		public static Quaternion SplineSegment(Quaternion q0 , Quaternion q1 , Quaternion q2 , Quaternion q3 , float t)
        {
			//Quaternion qa = Intermediate(q0, q1, q2);
			//Quaternion qb = Intermediate(q1, q2, q3);
			//return SQUAD(q1, qa, qb, q2, t);
			Quaternion result = GetPoint(q0, q1, q2, q3, t);
			return result;
		}

		// Tries to compute sensible tangent values for the quaternion
		public static Quaternion Intermediate(Quaternion q0 , Quaternion q1, Quaternion q2)
        {
			Quaternion q1inv = Quaternion.Inverse(q1);
			Quaternion c1 = q1inv * q2;
			Quaternion c2 = q1inv * q0;
			QuaternionEx.Log(ref c1);
			QuaternionEx.Log(ref c2);

			Quaternion c3 = QuaternionEx.Add(c2 , c1);
			QuaternionEx.Scale(ref c3 , 0.25f);
			QuaternionEx.Exp(ref c3);

			Quaternion r = q1 * c3;
			r.Normalize();
			return r;
		}

		public static Quaternion GetPoint(Quaternion q0, Quaternion q1, Quaternion q2, Quaternion q3, float t)
		{
			return Quaternion.Slerp(Quaternion.Slerp(q1, q2, t), Quaternion.Lerp(q0, q3, t), (float)(2.0 * t * (1.0 - t)));
		}
	}
}
