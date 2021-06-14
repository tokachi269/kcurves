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
        public override string Name { get; set; }

        //ユーザー制御点
        protected override List<ControlPoint> Knots { get; set; } = new List<ControlPoint>();
        public int TimePerRound { get; set; }
        protected List<ControlPoint> DefaultCameraPosition { get; set; }

        public IEnumerator Play()
        {
            for (float currentTime = 0; ;)
            {
                float dt = UnityEngine.Time.deltaTime;

                // 中心点centerの周りを、軸axisで、period周期で円運動
                transform.RotateAround(
                    Knots[0].position,
                    Vector3.up,
                    360 / TimePerRound * dt
                );

                Vector3 pos = CalcPosition(bezierIndex, t, IsLoop);
                Quaternion rot = CalcRotation(knotIndex, easing);
                moveCameraCube.transform.position = pos;
                moveCameraCube.transform.rotation = rot;
                GameObject.Find("Main Camera").transform.position = pos;
                GameObject.Find("Main Camera").transform.rotation = rot;

                float dt = UnityEngine.Time.deltaTime;

                currentTime += dt;

                yield return  null;
            }
        }

        public void RemoveKnot()
        {
            this.Knots.RemoveAt(Knots.Count - 1);
        }

        public void Render()
        {
            throw new NotImplementedException();
        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov)
        {
            this.Knots.Add(new ControlPoint(position, rotation, fov));
        }
    }
}
