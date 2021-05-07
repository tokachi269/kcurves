using Assets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BezierControls : MonoBehaviour
{
    public int N { get; private set; }
    //ユーザー制御点群
    public List<Knot> Knots = new List<Knot>();
    //ユーザー制御点群のt
    public double[] Ts { get; set; }
    //ベジェ制御点群
    //c_{0,0}, c_{0,1}, c_{1,0}, ..., c_{n-1,0}, c_{n-1,1}, c_{n-1,2}の順
    public Vector3[] Points { get; private set; }
    //パラメータtの弧長
    public float[,] Lengths { get; internal set; }

    public int SegmentCount { get; private set; }
    
    public float TotalLength { get; private set; }
    
    public bool IsCalcArcLengthWithT { get; private set; }

    public int ArcLengthWithTStep { get; private set; } = 100;


    //c_{i,j}
    public Vector3 this[int i, int j]
    {
        get => Points[2 * i + j];
        set => Points[2 * i + j] = value;
    }

    //コンストラクタ
    public BezierControls(int n, bool isLoop)
    {
        N = n;
        SegmentCount = n < 3 ? 1 : n;
        Points = new Vector3[2 * SegmentCount + 1];
        Lengths = new float[SegmentCount, ArcLengthWithTStep];
        Ts = new double[SegmentCount];
        IsCalcArcLengthWithT = false;
    }

    public Vector3[] CalcPlots( int stepPerSegment, bool isLoop, bool isSpce)
    {
        Vector3[] plots;
        if (N < 3)
            plots = new Vector3[stepPerSegment + 1];
        else
            plots = new Vector3[(isLoop ? N : (N - 2)) * stepPerSegment + 1];

        if (isSpce) CalcArcLengthWithT(isLoop);

        float t;
        int offset, k, j;
        int segCnt = isLoop ||SegmentCount < 3 ? SegmentCount : SegmentCount - 2;
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

    public Vector3 CalcPosition(bool isLoop, float t)
    {
        int seg = (int)Math.Truncate((t + 1) % SegmentCount);
        if (seg > SegmentCount)
        {
            seg = (isLoop || SegmentCount < 3) ? 1 : seg++;
        }
        return BezierUtil.CalcPosition(this[seg, 0], this[seg, 1], this[seg, 2], t % 1);
    }

    internal float GetT(float input)
    {
        if (input <= 0) return 0;
        int seg = 0;
        float inputL = input;
        for (int i= 0; i < SegmentCount-1; i++)
        {
            if (inputL <= Lengths[seg, ArcLengthWithTStep - 1])
            {
                break;
            }
            inputL -= Lengths[0, ArcLengthWithTStep - 1];
            seg++;
        }
        //int seg = (int)Math.Floor(input);
        //float inputL = (input - seg) * Lengths[seg, ArcLengthWithTStep - 1];
        int index= (int)Math.Floor( inputL/ Lengths[seg, ArcLengthWithTStep - 1] * (ArcLengthWithTStep-1));

        for (int i = 0; i < ArcLengthWithTStep-1; i++)
        {
            if ((index <= 0)||(index >= ArcLengthWithTStep - 1)) break;
            if (inputL <= Lengths[seg, index])
                if(Lengths[seg, index - 1] < inputL) break;
                else index--;
            else index++;
        }
         Debug.Log("input:" +input + "  inputL:" + inputL+ "  indexL:" + Lengths[seg, index]+ "  index:" + index);
        return seg + (float)((index - ((Lengths[seg, index] - inputL) / (Lengths[seg, index] - (index <= 0 ? 0 : Lengths[seg, index - 1]))))/ArcLengthWithTStep);
    }

    internal void CalcKnotsLength(bool isLoop)
    {
        for(int i = 1; i < SegmentCount; i++)
        {
            var frontL = BezierUtil.CalcBezierLength(this[i, 0], this[i-1, 1],  this[i-1, 0], Math.Floor(Ts[i]) - Ts[i-1]);
            
            int lastSeg;
            
            if (i != SegmentCount - 1)
                lastSeg = i + 1;
            else
                lastSeg = isLoop || SegmentCount < 3 ? 0 : i;

            var backL = BezierUtil.CalcBezierLength(this[i, 0], this[i, 1], this[lastSeg, 0], ((Ts[lastSeg] % 1 == 0) ? 1 : Ts[lastSeg] % 1));
            Knots[i].Length = frontL + backL;
        }
        //var seg = (isLoop || SegmentCount < 3) ? 0 : seg;
    }

    internal void CalcTotalKnotsLength()
    {
        float l = 0f;
        for (int i =0;i<SegmentCount;i++)
        {
            l += Lengths[i, ArcLengthWithTStep-1];
        }
        TotalLength = l;
    }

    /** ベジェ曲線のパラメータtと弧長のズレをパラメータ化 */
    public void CalcArcLengthWithT( bool isLoop)
    {

        int step = Lengths.GetLength(1);
        Vector3[] plots = CalcPlots(step,isLoop, false);
        int k;
        int segCnt = isLoop || SegmentCount < 3 ? SegmentCount - 1 : SegmentCount;
        for (k = 0; k < segCnt; k++)
        {
            float l = 0f;
            for (var i = 0; i < step; i++)
            {
                l += Vector3.Distance(plots[i], plots[i + 1]);
                Lengths[k, i] = l;
            }
        }
        for (k = 0; k < segCnt; k++)
        {
            float correctLength = BezierUtil.CalcBezierLength(this[k, 0], this[k, 1], this[k, 2], 1);
            for (var i = 0; i < step; i++)
            {
                Lengths[k, i] *= correctLength / Lengths[k, step-1];
            }
        }
        var last = isLoop || SegmentCount < 3 ? 0 : k*step;
        //Lengths[last, step-1] = 1f;
        //Lengths[k, step] += Vector3.Distance(plots[plots.Length], plots[last]);

        IsCalcArcLengthWithT = true;
    }
}