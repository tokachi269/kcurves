using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Mode.Rotate
{
    public class Rotate : BaseCameraMode, ICameraMode
    {
        protected override string Name { get; set; }

        //ユーザー制御点
        protected List<ControlPoint> Knots { get;  set; } = new List<ControlPoint>();
        public int Time { get; set; }
        protected List<ControlPoint> DefaultCameraPosition { get; private set; }

        public IEnumerator Play()
        {
            throw new NotImplementedException();
        }

        public void RemoveKnot()
        {
            this.Knots.RemoveAt(Knots.Count - 1);
        }

        public void Render()
        {
            throw new NotImplementedException();
        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov,bool lookAt)
        {
            this.Knots.Add(new ControlPoint(position, rotation, fov, false));
        }
    }
}
