using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CameraOperator.Tool
{
    public interface ICameraMode
    {
        IEnumerator Play();
        void AddKnot(Vector3 position, Quaternion rotation, float fov);
        void AddKnot(CameraConfig cp, float? param = null);
        void RemoveKnot();
    }

    public abstract class BaseCameraMode : MonoBehaviour
    {
        public abstract string Name { get; set; }
        //ユーザー制御点
        protected abstract List<CameraConfig> Knots { get; set; }

        public  int Time { get; set; }

        public bool IsCameraShake { get; set; }

        protected CameraConfig DefaultCameraPosition { get; }

    }
}
