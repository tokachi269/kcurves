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
        public ExtendBezierControls Beziers;
        public bool isLoop { get; private set; }

        public void SetBezierFromKnots()
        {
            Beziers = KCurves.CalcBezier( Knots, isLoop) as ExtendBezierControls;
            // plotsを計算
            //extendBezierControls.CalcPlots(step, isLoop, isEquallySpaced);
            //bezierResult.CalcKnotsLength(isLoop);
            //extendBezierControls.CalcTotalKnotsLength(isLoop);

            //tの移動距離を計算し、パラメータ化する
            //extendBezierControls.CalcArcLengthWithT(isLoop);
        }

        public Vector3[] DividePoints(bool isLoop, out int dividedSegmentCount)
        {                                                                                 
            dividedSegmentCount = (Beziers.SegmentCount - 2 ) * 2 + 2  ;
            Vector3[] dividedPoints = new Vector3[dividedSegmentCount * 2 + 1];
            int segCnt = isLoop || Beziers.SegmentCount < 3 ? Beziers.SegmentCount : Beziers.SegmentCount - 2;
            dividedPoints[0] = Beziers.Points[0];
            dividedPoints[0] = Beziers.Points[1];
            for (int i = 1; i <= segCnt; i++)
            {
                Debug.Log("i=" + i +","+ Beziers[i, 0] +","+ Beziers[i, 1] + "," + Beziers[i, 2] + "," + (float)Beziers.Ts[i]);
                Vector3[] result = BezierUtil.Divide(Beziers[i, 0], Beziers[i, 1], Beziers[i, 2], (float)Beziers.Ts[i]);

                for (int j = 0; j <= 3; j++)
                {
                    int index = 4 * (i - 1) + j + 2;
                    Debug.Log(index + ", "+result[j]);
                    if(index != 0 && index % 4 == 0)
                    {
                        dividedPoints[index] = Knots[index / 4].position;
                    }
                    else
                    {
                        dividedPoints[index] = result[j];

                    }
                }
            }
            dividedPoints[dividedPoints.Length - 3] = Beziers.Points[Beziers.Points.Length - 3];

            dividedPoints[dividedPoints.Length - 2] = Beziers.Points[Beziers.Points.Length - 2];
            dividedPoints[dividedPoints.Length - 1] = Beziers.Points[Beziers.Points.Length - 1];
            return dividedPoints;
        }

        public Vector3[] Output(int step, bool isLoop)
        {

            if (Beziers is null) {
                SetBezierFromKnots();
            }
            if (Beziers.SegmentCount > 3  (Beziers.SegmentCount == (Knots.Count < 3 ? 1 : Knots.Count)))
            {
                int segment;
                Vector3[] divVec= DividePoints(isLoop, out segment);
                Beziers = new ExtendBezierControls(segment, divVec, isLoop);
            }

            Debug.Log("x segCnt"+Beziers.SegmentCount);
            Debug.Log("x length"+Beziers.Points.Length);
            return Beziers.CalcPlots(step, isLoop);
        }


        public Vector3 CalcPosition(bool isLoop, float t)
        {            
            int segIndex = (int)Math.Truncate((t + 1) % Beziers.SegmentCount);
            if (segIndex > Beziers.SegmentCount)
            {
                segIndex = (isLoop || Beziers.SegmentCount < 3) ? 1 : segIndex++;
            }
            return BezierUtil.CalcPosition(Beziers[segIndex, 0], Beziers[segIndex, 1], Beziers[segIndex, 2], t % 1);
        }


        public Quaternion CalcRotation(int segIndex, float inputL)
        {
            float t = inputL / Beziers.Lengths[segIndex, Beziers.ArcLengthWithTStep - 1];
            int nextSegIndex = (segIndex < Beziers.SegmentCount ? segIndex + 1 : segIndex);

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
            if (!Beziers.IsCalcArcLengthWithT) Beziers.CalcArcLengthWithT(isLoop);

            return Beziers.TotalLength / time * 0.01f;
        }
    }
}