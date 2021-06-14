using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Mode
{
    class CameraUtil
    {
        public static ControlPoint CameraPosition()
        {
            return new ControlPoint(Camera.main.transform.position, Camera.main.transform.rotation, Camera.main.fieldOfView);
        }

        /*P2をP1から回転が少ない方向に回転するように補正する*/
        public static Vector3 ClosestAngle(Vector3 basePoint, Vector3 point)
        {
            Vector3 A = basePoint - point;
            if (A.x > 180f)
            {
                point.x += 360f;
            }
            else if (A.x < -180f)
            {
                point.x -= 360f;
            }
            if (A.y > 180f)
            {
                point.y += 360f;
            }
            else if (A.y < -180f)
            {
                point.y -= 360f;
            }
            if (A.z > 180f)
            {
                point.z += 360f;
            }
            else if (A.z < -180f)
            {
                point.z -= 360f;
            }
            return point;
        }
    }
}
