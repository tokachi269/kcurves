using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public class ExtendBezierControls : BezierControls
    {
        //パラメータtの弧長
        public float[,] Lengths { get; internal set; }

        public float TotalLength { get; private set; }

        public bool IsCalcArcLengthWithT { get; private set; }

        public int ArcLengthWithTStep { get; private set; } = 10;


        //コンストラクタ
        public ExtendBezierControls(int n, bool isLoop) : base(n, isLoop)
        {
            Lengths = new float[SegmentCount - 1, ArcLengthWithTStep];
            IsCalcArcLengthWithT = false;
        }

        public void DividePoints(bool isLoop)
        {
            int dividedSegmentCount = SegmentCount * 2 - 2;
            Vector3[] dividedPoints = new Vector3[dividedSegmentCount];
            int segCnt = isLoop || SegmentCount < 3 ? SegmentCount - 1 : SegmentCount;
            for (int k = 0; k < segCnt; k++)
            {
                Vector3[] result = BezierUtil.Divide(this[k, 0], this[k, 1], this[k + 1, 0], (float)Ts[k]);
                ;
                for (int i = 0; i <= 4; i++)
                {
                    dividedPoints[i * k + i] = result[i];
                }
                SegmentCount = dividedSegmentCount;
            }
        }

        internal float GetT(ref int segIndex, ref float inputL)
        {

            if (inputL <= 0) return 0f;
            if (inputL >= Lengths[segIndex, ArcLengthWithTStep - 1])
            {
                if (segIndex <= SegmentCount - 2)
                {
                    inputL -= Lengths[segIndex, ArcLengthWithTStep - 1];
                    segIndex++;
                }
            }
            //int seg = (int)Math.Floor(input);
            //float inputL = (input - seg) * Lengths[seg, ArcLengthWithTStep - 1];
            int index = (int)Math.Floor(inputL / Lengths[segIndex, ArcLengthWithTStep - 1] * (ArcLengthWithTStep - 1));

            for (int i = 0; i < ArcLengthWithTStep - 1; i++)
            {
                if ((index <= 0) || (index >= ArcLengthWithTStep - 1)) break;
                if (inputL <= Lengths[segIndex, index])
                    if (Lengths[segIndex, index - 1] < inputL) break;
                    else index--;
                else index++;
            }

            //Debug.Log("input:" +input + "  inputL:" + inputL+ "  indexL:" + Lengths[seg, index]+ "  index:" + index);
            float resultL = index - ((Lengths[segIndex, index] - inputL) / (Lengths[segIndex, index] - (index <= 0 ? 0 : Lengths[segIndex, index - 1])));
            float resultT = segIndex + (float)(resultL / ArcLengthWithTStep);
            return resultT;
        }

        internal void CalcTotalKnotsLength()
        {
            float l = 0f;
            for (int i = 0; i < SegmentCount - 1; i++)
            {
                l += Lengths[i, ArcLengthWithTStep - 1];
            }
            TotalLength = l;
        }

        /** ベジェ曲線のパラメータtと弧長のズレをパラメータ化 */
        public void CalcArcLengthWithT(bool isLoop)
        {
            Vector3[] plots = CalcPlots(ArcLengthWithTStep, isLoop);
            int k;
            int segCnt = isLoop || SegmentCount < 3 ? SegmentCount : SegmentCount - 2;
            for (k = 0; k <= segCnt; k++)
            {
                float l = 0f;
                for (var i = 0; i < ArcLengthWithTStep; i++)
                {
                    l += Vector3.Distance(plots[i], plots[i + 1]);
                    Lengths[k, i] = l;
                }
            }
            for (k = 0; k <= segCnt; k++)
            {
                float correctLength = BezierUtil.CalcLength(this[k, 0], this[k, 1], this[k, 2], 1);
                for (var i = 0; i < ArcLengthWithTStep; i++)
                {
                    Lengths[k, i] *= correctLength / Lengths[k, ArcLengthWithTStep - 1];
                }
            }
            var last = isLoop || SegmentCount < 3 ? 0 : k * ArcLengthWithTStep;

            //Lengths[last, step-1] = 1f;
            //Lengths[k, step] += Vector3.Distance(plots[plots.Length], plots[last]);
            
            CalcTotalKnotsLength();
            IsCalcArcLengthWithT = true;
        }
        public Vector3[] CalcPlots(int stepPerSegment, bool isLoop)
        {
            Vector3[] plots;
            if (SegmentCount < 3)
                plots = new Vector3[stepPerSegment + 1];
            else
                plots = new Vector3[(isLoop ? SegmentCount : (SegmentCount - 2)) * stepPerSegment + 1];

             //CalcArcLengthWithT(isLoop);

            float t;
            int offset, k;
            int segCnt = isLoop || SegmentCount < 3 ? SegmentCount : SegmentCount - 2;
            for (k = 0; k < segCnt; k++)
            {
                offset = k * stepPerSegment;
                var nextk = (k + 1) % SegmentCount;
                for (var i = 0; i < stepPerSegment; i++)
                {
                    t = i / (float)stepPerSegment;

                    plots[offset + i] = BezierUtil.CalcPosition(this[nextk, 0], this[nextk, 1], this[nextk, 2], t);
                }
            }
            var last = isLoop || SegmentCount < 3 ? 0 : k;
            plots[plots.Length - 1] = BezierUtil.CalcPosition(this[last, 0], this[last, 1], this[last, 2], 1);
            return plots;
        }

        //不要
        internal void CalcKnotsLength(bool isLoop)
        {
            for (int i = 1; i < SegmentCount; i++)
            {
                var frontL = BezierUtil.CalcLength(this[i, 0], this[i - 1, 1], this[i - 1, 0], Math.Floor(Ts[i]) - Ts[i - 1]);

                int lastSegIndex;

                if (i != SegmentCount - 1)
                    lastSegIndex = i + 1;
                else
                    lastSegIndex = isLoop || SegmentCount < 3 ? 0 : i;

                var backL = BezierUtil.CalcLength(this[i, 0], this[i, 1], this[lastSegIndex, 0], ((Ts[lastSegIndex] % 1 == 0) ? 1 : Ts[lastSegIndex] % 1));
                //Knots[i].Length = frontL + backL;
            }
            //var seg = (isLoop || SegmentCount < 3) ? 0 : seg;
        }
    }
}