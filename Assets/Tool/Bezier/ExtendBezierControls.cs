using System;
using System.Linq;
using UnityEngine;

namespace CamOpr.Tool
{

    public class ExtendBezierControls : BezierControls
    {
        //パラメータtの弧長
        public float[,] Lengths { get; internal set; }

        public float TotalLength { get; private set; }

        public bool IsCalcTotalLength { get; private set; }

        public ushort ArcLengthWithTStep { get; private set; } = 6;

        public ExtendBezierControls(int n, bool isLoop) : base(n, isLoop)
        {
            Lengths = new float[SegmentCount <= 1 ? 1 : SegmentCount-2, ArcLengthWithTStep];
            IsCalcTotalLength = false;
        }

        public ExtendBezierControls(int n, Vector3[] points, bool isLoop) : base(n, isLoop)
        {
            this.Points = points;
            Lengths = new float[SegmentCount <= 1 ? 1 : SegmentCount, ArcLengthWithTStep];
        }

        internal float GetT(int bezierIndex, float progressLength,Vector3 befPosition, float befDistance)
        {
            int index = Mathf.Clamp((int)Math.Floor(progressLength / Lengths[bezierIndex, ArcLengthWithTStep - 1] * (ArcLengthWithTStep - 1)), 0, ArcLengthWithTStep - 1);

            for (int i = 0; i < ArcLengthWithTStep - 1; i++)
            {
                if ((index <= 0) || (index >= ArcLengthWithTStep - 1)) { break; }

                if (progressLength <= Lengths[bezierIndex, index]){
                    if (Lengths[bezierIndex, index - 1] < progressLength){ break; }
                    else{index--; }
                }
                else{ index++; }
            }
            
            int[] array = BezierUtils.GetSplineIndex(ArcLengthWithTStep, index);
            //Debug.Log(string.Join(", ", array));
            float a = Lengths[bezierIndex, index] - progressLength;
/*            float a = (float)BezierUtils.Spline(
                (Lengths[bezierIndex, index] - progressLength) / (Lengths[bezierIndex, index] - (index == 0 ? 0 : Lengths[bezierIndex, index - 1])),
                Lengths[bezierIndex, array[0]],
                Lengths[bezierIndex, array[1]],
                Lengths[bezierIndex, array[2]],
                Lengths[bezierIndex, array[3]]
                );*/

            if (a<0)
            {
                Debug.Log("マイナスだよ");
                Debug.Log("Lengths[bezierIndex, index] - progressLength):"+ (Lengths[bezierIndex, index] - progressLength));

            }
            //float a =( Lengths[bezierIndex, index] - progressLength) / (Lengths[bezierIndex, index] - (index <= 0 ? 0 : Lengths[bezierIndex, index - 1]));
            float resultL = 1 + index - a;
            //float resultL = 1+index - ((Lengths[bezierIndex, index] - progressLength) / (Lengths[bezierIndex, index] - (index <= 0 ? 0 : Lengths[bezierIndex, index - 1])));
            float resultT = (float)(resultL / ArcLengthWithTStep);
            float baseT = resultT;
            //if (befDistance != -1) Debug.Log("resultT調整前：" + resultT);

            if (false)
            {
                float conversion = 1f / ArcLengthWithTStep;
                //Debug.Log(conversion);
                for (int i = 2; i < 50; i++)
                {
                    //Debug.Log("resultT:" + resultT);
                    var position = BezierUtils.Position(this[bezierIndex, 0], this[bezierIndex, 1], this[bezierIndex, 2], resultT);

                    float currentDistance = Vector3.Distance(befPosition, position);

                    //Debug.Log("差分:" + befDistance + "/" + currentDistance + "=" + befDistance / currentDistance);
                    //if (befDistance / currentDistance > 0.99 && befDistance / currentDistance < 1.01)
                    //{
                    //    break;
                    //}
                    //Debug.Log("再計算:"+ conversion * Math.Sign(currentDistance)+ "Math.Sign(currentDistance)" + Math.Sign(currentDistance) );

                    conversion /= 2;
                    if (befDistance < currentDistance)
                    {
                         resultT = Mathf.Clamp(resultT - conversion * Math.Sign(currentDistance / befDistance), -100, 100);
                    }
                    else
                    {
                         resultT = Mathf.Clamp(resultT + conversion * Math.Sign(currentDistance / befDistance), -100, 100);
                    }

                }
            }
            //if (befDistance != -1) Debug.Log("resultL調整後：" + resultT);
            if (befDistance != -1)
            {
                // Debug.Log(string.Join(", ", array));

                Debug.Log("bezierIndex:" + bezierIndex + " index:" + index + " a:"+ a + " resultL:" + resultL + " baseT:" + baseT + " resultT:" + resultT);
                if (resultT < (Lengths[bezierIndex, index] - progressLength) / (Lengths[bezierIndex, index] - (index <= 0 ? 0 : Lengths[bezierIndex, index - 1])))
                {
                    //TODO indexがArcLengthWithTStepを超えて0に戻ったときにprogressLengthが正しくない？
                    Debug.Log(string.Join(", ", array));
                    Debug.Log("逆転");
                }
            }

            return resultT;
        }

        public float Length(int bezierIndex) 
        {
            return Lengths[bezierIndex, ArcLengthWithTStep-1];
        }

        public void SetParameter(bool isLoop)
        {
            CalcArcLengthWithT(isLoop);
            TotalLength = Lengths.Cast<float>().Where((n, i) => ((i + 1) % ArcLengthWithTStep) == 0).Sum();
            Debug.Log("TotalLength" + TotalLength);
            IsCalcTotalLength = true;
        }

        /** ベジェ曲線のパラメータtと弧長のズレをパラメータ化 */
        public void CalcArcLengthWithT(bool isLoop)
        {
            Vector3[] plots = CalcPlots(ArcLengthWithTStep, isLoop);
            // TODO SegmentCountが1、２のとき
            for (ushort i = 0; i < SegmentCount; i++)
            {
                if((this[i, 0] == this[i, 1]) || (this[i, 0] == this[i, 2]))
                {
                    continue;
                }
                else
                {
                    float l = 0f;
                    for (ushort j = 0; j < ArcLengthWithTStep; j++)
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
        }

        public Vector3[] CalcPlots(ushort stepPerSegment, bool isLoop)
        {
            Vector3[] plots;
            float t;
            int offset, i;
            if (SegmentCount <= 2)
            {
                plots = new Vector3 [SegmentCount];
                plots[0] = this[0, 0];
                if (SegmentCount == 2) plots[1] = this[1, 0];
                return plots;
            }

            int segCnt = isLoop ? SegmentCount + 1 : SegmentCount;
            plots = new Vector3[Math.Max(0, segCnt) * stepPerSegment + (isLoop ? 0:1)];


            for (i = 0; i < segCnt; i++)
            {
                offset = i * stepPerSegment;
                for (var j = 0; j < stepPerSegment; j++)
                {
                    t = j / (float)stepPerSegment;

                    plots[offset + j] = BezierUtils.Position(this[i, 0], this[i, 1], this[i, 2], t);
                }
            }
            if (isLoop) {
                plots[plots.Length - 1] = this[0, 0]; 
            }
            else
            {
                var last = isLoop ? 0 : i - 1;
                plots[plots.Length - 1] = BezierUtils.Position(this[last, 0], this[last, 1], this[last, 2], 1);
            }
            return plots;
        }

    }

}