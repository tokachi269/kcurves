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
            return new ControlPoint(Camera.main.transform.position, Camera.main.transform.rotation, 60, false);
        }
    }
}
