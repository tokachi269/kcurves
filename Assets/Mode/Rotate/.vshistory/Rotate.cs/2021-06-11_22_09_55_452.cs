using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Mode.Rotate
{
    public class Rotate : MonoBehaviour, ICameraMode
    {
        public List<ControlPoint> Knots => throw new NotImplementedException();

        public void AddLookAt(Vector3 position, Quaternion rotation, float fov)
        {
            throw new NotImplementedException();
        }

        public IEnumerator Play()
        {
            throw new NotImplementedException();
        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov, bool lookAt)
        {
            this.Knots.Add(new ControlPoint(position, rotation, fov, lookAt));
            if (Knots.Count == 0)
            {
                moveCameraCube.transform.position = Knots[0].position;
            }
            SetBezierFromKnots();
        }

        public void RemoveKnot()
        {
            this.Knots.RemoveAt(Knots.Count - 1);
            SetBezierFromKnots();
        }

        public void AddLookAt(Vector3 position, Quaternion rotation, float fov)
        {
            this.LookAts.Add(new ControlPoint(position, rotation, fov, false));
            SetBezierFromKnots();
        }

        public void RemoveLookAt(Vector3 position, Quaternion rotation, float fov)
        {
            this.LookAts.RemoveAt(Knots.Count - 1);
            SetBezierFromKnots();
        }
        public void Render()
        {
            throw new NotImplementedException();
        }
    }
}
