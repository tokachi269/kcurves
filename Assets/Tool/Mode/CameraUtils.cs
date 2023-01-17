using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CamOpr.Tool
{
    public static class CameraUtils
    {
        public static CameraConfig CameraPosition()
        {
            return new CameraConfig(Camera.main.transform.position, Camera.main.transform.rotation, Camera.main.fieldOfView);
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

        internal static void SetCamera(CameraConfig cp)
        {
            Camera.main.transform.position = cp.Position;
            Camera.main.transform.rotation = cp.Rotation;
            Camera.main.fieldOfView = cp.Fov;
        }
    }
}
