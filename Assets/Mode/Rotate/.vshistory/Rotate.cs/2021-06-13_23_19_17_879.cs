﻿using System;
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
        protected ControlPoint DefaultPosition { get; set; }
        private static GameObject moveCameraCube;

        public IEnumerator Play()
        {
            DefaultPosition = CameraUtil.CameraPosition();

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
