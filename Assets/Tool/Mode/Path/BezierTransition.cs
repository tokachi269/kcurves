using System.Collections.Generic;
using UnityEngine;

namespace CamOpr.Tool
{
    public class BezierTransition
    {
        public float CurrentProgressLength;
        public float KnotsBetweenLength = 0f;
        public List<CameraConfig> tempKnots;
        private ExtendBezierControls Positions;
        public int currentKnotIndex = 0;
        public int currentBezierIndex = 0;

        public float diffProgressLength = 0;
        private float befProgressLength = 0;

        public float diffProgressT = 0;
        private float befProgressT = 0;

        public BezierTransition()
        {

        }

        public BezierTransition(List<CameraConfig> knots, ExtendBezierControls positions)
        {
            this.tempKnots = knots;
            this.Positions = positions;
            this.KnotsBetweenLength = positions.Length(0);
        }
        public void Play(ref float progressLength,int targetKnotIndex,int targetBezierIndex,float t)
        {
            Play(progressLength, targetKnotIndex, targetBezierIndex);

            progressLength = CurrentProgressLength;
            DebugDifCalculation(progressLength,t);
            // targetKnotIndex = currentKnotIndex;
            // targetBezierIndex = currentBezierIndex;
        }

        private void Play(float progressLength,int targetKnotIndex,int targetBezierIndex)
        {  
            // 次のセグメントに移動したか判定する
            bool isSegChanged = false;
            if (targetBezierIndex == 0 || targetBezierIndex == Positions.SegmentCount)
            {
                if (progressLength >= KnotsBetweenLength)
                {
                    isSegChanged = true;
                }
            }
            else if (targetBezierIndex % 2 == 1)
            {
                if (progressLength >= Positions.Length(targetBezierIndex))
                {
                    isSegChanged = true;
                }
            }
            else
            {
                if (progressLength >= Positions.Length(targetBezierIndex - 1) + Positions.Length(targetBezierIndex))
                {
                    //progressLength -= Positions.Length(targetBezierIndex - 1) + Positions.Length(targetBezierIndex);
                    isSegChanged = true;
                }
            }

            //セグメントが移動した場合、計算に必要なパラメーターを設定する
            if (isSegChanged)
            {
                targetBezierIndex++;

                targetKnotIndex = targetBezierIndex % 2 == 0 ? (targetBezierIndex / 2) : (targetBezierIndex + 1) / 2;
                if (targetBezierIndex % 2 == 1)
                {
                    progressLength -= KnotsBetweenLength;

                    //Knot間の距離を求める
                    KnotsBetweenLength = 0;
                    if (targetKnotIndex == tempKnots.Count - 1)
                    {
                        KnotsBetweenLength = Positions.Length(targetBezierIndex);
                    }
                    else
                    {
                        for (ushort j = (ushort)(2 * targetKnotIndex - 1); j < Positions.SegmentCount && j <= 2 * targetKnotIndex; j++)
                        {
                            KnotsBetweenLength += Positions.Length(j);
                        }
                    }
                }

                Debug.Log("targetBezierIndex: " + targetBezierIndex);
            }

            CurrentProgressLength = progressLength;
            currentKnotIndex = targetKnotIndex;
            currentBezierIndex = targetBezierIndex ;

        }

        private void DebugDifCalculation(float progressLength,float t)
        {
           diffProgressLength = progressLength - befProgressLength;
           befProgressLength = progressLength;

           diffProgressT = t - befProgressT;
           befProgressT = t;
        }
    }
}
