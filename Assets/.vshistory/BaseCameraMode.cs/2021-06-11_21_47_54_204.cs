using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public interface ICameraMode
    {
        string Name { get; }
        int Time { get; }
        List<ControlPoint> Knots { get; }
        IEnumerator MoveCamera();
        void AddLookAt(Vector3 position, Quaternion rotation, float fov);
        void RemoveKnot();
        void Render();
    }

    class BaseCameraMode: MonoBehaviour, ICameraMode
    {
    }
}
