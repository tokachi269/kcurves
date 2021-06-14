using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public interface ICameraMode
    {
        List<ControlPoint> Knots { get; }
        IEnumerator Play();
        void AddLookAt(Vector3 position, Quaternion rotation, float fov);
        void RemoveKnot();
        void Render();
    }

}
