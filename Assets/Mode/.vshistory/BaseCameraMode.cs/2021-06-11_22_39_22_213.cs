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
        IEnumerator Play();
        void AddKnot(Vector3 position, Quaternion rotation, float fov, bool lookAt);
        void RemoveKnot();
        void Render();
    }
    public abstract class BaseCameraMode : MonoBehaviour
    {
        protected abstract string Name { get; set; }
        //ユーザー制御点
        protected abstract List<ControlPoint> Knots { get; set; }
        public int Time { get; set; }
        ControlPoint DefaultCameraPosition { get; }

    }
}
