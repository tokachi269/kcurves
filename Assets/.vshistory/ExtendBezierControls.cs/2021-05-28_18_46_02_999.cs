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


        public ExtendBezierControls(int n, bool isLoop) : base(n, isLoop)
        {
            Lengths = new float[SegmentCount <= 1 ? 1 : SegmentCount-2, ArcLengthWithTStep];
            IsCalcArcLengthWithT = false;
        }

        public ExtendBezierControls(int n, Vector3[] points, bool isLoop) : base(n, isLoop)
        {
            this.Points = points;
            Lengths = new float[SegmentCount <= 1 ? 1 : SegmentCount, ArcLengthWithTStep];
        }

        internal float GetT(int bezierIndex, out int nextBezierIndex, ref float inputL)
        {
            if (inputL <= 0)
            {
                nextBezierIndex = bezierIndex;
                return 0f;
            }
            if ( inputL >= Lengths[bezierIndex, ArcLengthWithTStep - 1])
            {
                if (bezierIndex <= SegmentCount - 1)
                {
                    inputL -= Lengths[bezierIndex, ArcLengthWithTStep - 1];
                    bezierIndex++;
                    if (bezierIndex == SegmentCount)
                    {
                        nextBezierIndex = bezierIndex;
                        return SegmentCount;
                    }
                }
            }
            nextBezierIndex = bezierIndex;
            int index = Mathf.Clamp((int)Math.Floor(inputL / Lengths[bezierIndex, ArcLengthWithTStep - 1] * (ArcLengthWithTStep - 1)),0, ArcLengthWithTStep - 1);

            for (int i = 0; i < ArcLengthWithTStep - 1; i++)
            {
                if ((index <= 0) || (index >= ArcLengthWithTStep - 1)) break;
                if (inputL <= Lengths[bezierIndex, index])
                    if (Lengths[bezierIndex, index - 1] < inputL) break;
                    else index--;
                else index++;
            }
            //Debug.Log("segIndex"+ segIndex + "  index:" + index);
            //Debug.Log("  inputL:" + inputL+ "  indexL:" + Lengths[segIndex, index]+ "  index:" + index);
            float resultL = 1+index - ((Lengths[bezierIndex, index] - inputL) / (Lengths[bezierIndex, index] - (index <= 0 ? 0 : Lengths[bezierIndex, index - 1])));
            float resultT = bezierIndex + (float)(resultL / ArcLengthWithTStep);
            return resultT;
        }


        /** ベジェ曲線のパラメータtと弧長のズレをパラメータ化 */
        public void CalcArcLengthWithT(int count ,bool isLoop)
        {
            Vector3[] plots = CalcPlots(count, ArcLengthWithTStep, isLoop);
            // TODO SegmentCountが1、２のとき
            for (int i = 0; i < Lengths.GetLength(0); i++)
            {
                if((this[i, 0] == this[i, 1]) || (this[i, 0] == this[i, 2]))
                {
                    continue;
                }
                else
                {
                    float l = 0f;
                    for (var j = 0; j < ArcLengthWithTStep; j++)
                    {
                        l += Vector3.Distance(plots[i * ArcLengthWithTStep + j], plots[i * ArcLengthWithTStep + j + 1]);
                        Lengths[i, j] = l;
                    }
                }

            }

            //TODO 微分の結果が正しくない
            //for (int i = 0; i < Lengths.GetLength(0); i++)
            //{
            //    float correctLength = BezierUtil.CalcLength(this[i, 0], this[i, 1], this[i, 2], 1);
            //    Debug.Log("index：" + i + " 微分：" + correctLength + "  分割：" + Lengths[i, ArcLengthWithTStep - 1]);
            //    for (var j = 0; j < ArcLengthWithTStep; j++)
            //    {
            //        Lengths[i, j] *= correctLength / Lengths[i, ArcLengthWithTStep - 1];
            //    }
            //}

            //Lengths[last, step-1] = 1f;
            //Lengths[k, step] += Vector3.Distance(plots[plots.Length], plots[last]);
            TotalLength = Lengths.Cast<float>().Where((n, i) => i != 0 && (i % ArcLengthWithTStep - 1) == 0).Sum();
            IsCalcArcLengthWithT = true;
        }
        public Vector3[] CalcPlots(int count,int stepPerSegment, bool isLoop)
        {
            Debug.Log(SegmentCount);
            Debug.Log(count);

            Vector3[] plots;
            if (SegmentCount < 3)
                plots = new Vector3[stepPerSegment + 1];
            else
                plots = new Vector3[(isLoop ? SegmentCount : SegmentCount) * stepPerSegment + 1];

             //CalcArcLengthWithT(isLoop);

            float t;
            int offset, i;
            int segCnt = isLoop || SegmentCount < 3 ? SegmentCount+1 : SegmentCount;
            for (i = 0; i < segCnt; i++)
            {
                offset = i * stepPerSegment;
                var nextI = i % SegmentCount;
                for (var j = 0; j < stepPerSegment; j++)
                {
                    t = j / (float)stepPerSegment;

                    plots[offset + j] = BezierUtil.Position(this[nextI, 0], this[nextI, 1], this[nextI, 2], t);
                }
            }
            var last = isLoop || SegmentCount < 3 ? 0 : i-1;
            plots[plots.Length - 1] = BezierUtil.Position(this[last, 0], this[last, 1], this[last, 2], 1);
            return plots;
        }

    }
}