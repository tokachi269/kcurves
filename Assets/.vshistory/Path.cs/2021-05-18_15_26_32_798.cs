using Assets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Path : MonoBehaviour
    {
        public string Name { get; private set; }
        //ユーザー制御点
        public List<ControlPoint> Knots = new List<ControlPoint>();
        //ユーザー制御注視点
        public List<ControlPoint> LookAts = new List<ControlPoint>();
        //Bezier計算結果
        public ExtendBezierControls extendBezierControls;
        public bool isLoop { get; private set; }

        public void SetBezierFromKnots()
        {
            extendBezierControls = KCurves.CalcBezier( Knots, isLoop) as ExtendBezierControls;
            // plotsを計算
            //extendBezierControls.CalcPlots(step, isLoop, isEquallySpaced);
            //bezierResult.CalcKnotsLength(isLoop);
            //extendBezierControls.CalcTotalKnotsLength(isLoop);
            if (extendBezierControls.SegmentCount >= 3)
            {
                extendBezierControls.DividePoints(isLoop);
            }
            //tの移動距離を計算し、パラメータ化する
            //extendBezierControls.CalcArcLengthWithT(isLoop);
        }

        public Vector3[] Output(int step, bool isLoop)
        {
            if (extendBezierControls is null) SetBezierFromKnots();
            return extendBezierControls.CalcPlots(step, isLoop);
        }


        public Vector3 CalcPosition(bool isLoop, float t)
        {            
            int segIndex = (int)Math.Truncate((t + 1) % extendBezierControls.SegmentCount);
            if (segIndex > extendBezierControls.SegmentCount)
            {
                segIndex = (isLoop || extendBezierControls.SegmentCount < 3) ? 1 : segIndex++;
            }
            return BezierUtil.CalcPosition(extendBezierControls[segIndex, 0], extendBezierControls[segIndex, 1], extendBezierControls[segIndex, 2], t % 1);
        }



        public Quaternion CalcRotation(int segIndex, float inputL)
        {
            float t = inputL / extendBezierControls.Lengths[segIndex, extendBezierControls.ArcLengthWithTStep - 1];
            int nextSegIndex = (segIndex < extendBezierControls.SegmentCount ? segIndex + 1 : segIndex);

            Quaternion rotation;
            if (!Quaternion.Equals(Knots[segIndex].rotation, Knots[nextSegIndex].rotation))
            {
                rotation = Quaternion.Lerp(Knots[segIndex].rotation, Knots[nextSegIndex].rotation, t);
            }
            else
            {
                rotation = Knots[segIndex].rotation;
            }
            if (LookAts.Count != 0)
            {
                if (Knots[segIndex].isLookAt || Knots[nextSegIndex].isLookAt)
                {
                    rotation = Quaternion.Lerp((Knots[segIndex].isLookAt ? Quaternion.LookRotation(LookAts[0].position) : rotation),
                                           (Knots[nextSegIndex].isLookAt ? Quaternion.LookRotation(LookAts[0].position) : rotation), t);
                }
            }
            return rotation;
        }
        public void AddKnot(Vector3 position, Quaternion rotation, float fov, bool lookAt)
        {
            this.Knots.Add(new ControlPoint(position, rotation, fov, lookAt));
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

        internal float MaxSpeed(int time)
        {
            if (!extendBezierControls.IsCalcArcLengthWithT) extendBezierControls.CalcArcLengthWithT(isLoop);

            return extendBezierControls.TotalLength / time * 0.01f;
        }
    }
}