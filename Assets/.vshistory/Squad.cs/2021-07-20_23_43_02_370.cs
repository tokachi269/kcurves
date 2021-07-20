using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    class Squad
    {
		// Returns a smoothed quaternion along the set of quaternions making up the spline, each quaternion is along an equidistant value in t
		public Quaternion Spline(quaternions as (Quaternion),int count,float  t )
        {
			int section = (quaternions.Length - 1) * t;
			float alongLine = (quaternions.Length - 1) * t - section;
			
			if (section == 0)
			{
				return SplineSegment(quaternions[section], quaternions[section], quaternions[section + 1], quaternions[section + 2], alongLine);

			}
			else if (section == quaternions.Length - 2 and section > 0){
				return SplineSegment(quaternions[section - 1], quaternions[section], quaternions[section + 1], quaternions[section + 1], alongLine);

			}
			else if (section >= 1 and section < quaternions.Length - 2){
				return SplineSegment(quaternions[section - 1], quaternions[section], quaternions[section + 1], quaternions[section + 2], alongLine);

			}
		}

		// Returns a quaternion between q1 and q2 as part of a smooth SQUAD segment
		public Quaternion SplineSegment(Quaternion q0 , Quaternion q1 , Quaternion q2 , Quaternion q3 , float t)
        {
			Quaternion qa = Intermediate(q0, q1, q2);
			Quaternion qb = Intermediate(q1, q2, q3);
			return SQUAD(q1, qa, qb, q2, t);

		}


		// Tries to compute sensible tangent values for the quaternion
		public Quaternion Intermediate(Quaternion q0 , Quaternion q1, Quaternion q2)
        {
			Quaternion q1inv = Quaternion.Inverse(q1);
			Quaternion c1 = q1inv * q2;
			Quaternion c2 = q1inv * q0;
			QuaternionEx.Log(ref c1);
			QuaternionEx.Log(ref c2);

			Quaternion c3 = c2 * c1;
			QuaternionEx.Scale(ref c3 , - 0.25f);
			QuaternionEx.Exp(ref c3);

			Quaternion r = q1 * c3;
			r.Normalize();
			return r;
		}


		// Returns a smooth approximation between q1 and q2 using t1 and t2 as 'tangents'
		public Quaternion SQUAD(Quaternion q1 , Quaternion t1 , Quaternion t2 , Quaternion q2 ,float  t )
        {
			float slerpT = 2.0f * t * (1.0f - t);
			Quaternion slerp1 = QuaternionEx.SlerpNoInvert(q1, q2, t);
			Quaternion slerp2 = QuaternionEx.SlerpNoInvert(t1, t2, t);
			return QuaternionEx.SlerpNoInvert(slerp1, slerp2, slerpT);

		}


	}
}
