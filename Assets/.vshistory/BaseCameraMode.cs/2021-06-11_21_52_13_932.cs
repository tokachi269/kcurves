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

        List<ControlPoint> Knots { get; }
        IEnumerator MoveCamera();
        void AddLookAt(Vector3 position, Quaternion rotation, float fov);
        void RemoveKnot();
        void Render();
    }

    public abstract class BaseCameraMode: MonoBehaviour, ICameraMode
    {
        public string Name { get; private set; }
        public int Time { get; private set; }
    }
}
