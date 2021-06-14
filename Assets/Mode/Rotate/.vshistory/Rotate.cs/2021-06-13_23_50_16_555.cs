using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class Rotate : BaseCameraMode, ICameraMode
    {
        public override string Name { get; set; }

        //ユーザー制御点
        protected override List<ControlPoint> Knots { get; set; } = new List<ControlPoint>();
        public int TimePerRound { get; set; }
        public bool IsCameraShake { get; set; }

        protected ControlPoint DefaultPosition { get; set; }
        private GameObject moveCameraCube;

        public void Start()
        {
            moveCameraCube = new GameObject("moveCameraCube");
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.parent = this.transform;
        }

        public IEnumerator Play()
        {
            DefaultPosition = CameraUtil.CameraPosition();
            if (TimePerRound==0)
            {
                Debug.LogError("The cycle time cannot be set to zero.");
            }

            for (float currentTime = 0; ;)
            {
                float dt = UnityEngine.Time.deltaTime;

                transform.RotateAround(
                    Knots[0].position,
                    Vector3.up,
                    360 / TimePerRound * dt
                );

                currentTime += dt;
                yield return  null;
            }

            moveCameraCube.transform.position = DefaultPosition.position;
            moveCameraCube.transform.rotation = DefaultPosition.rotation;
            yield break;
        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov)
        {
            this.Knots.Add(new ControlPoint(position, rotation, fov));
            if (Knots.Count == 0)
            {
                moveCameraCube.transform.position = Knots[0].position;
            }
        }

        public void RemoveKnot()
        {
            this.Knots.RemoveAt(Knots.Count - 1);
        }


        public void AddKnot(ControlPoint cp)
        {
            this.Knots.Add(cp);
            if (Knots.Count == 0)
            {
                moveCameraCube.transform.position = Knots[0].position;
            }
        }


        public void Render()
        {
            throw new NotImplementedException();
        }
    }
}
