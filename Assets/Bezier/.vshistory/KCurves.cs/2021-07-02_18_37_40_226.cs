
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public static class KCurves
    {
        public static BezierControls CalcBeziers(List<ControlPoint> Knots, int iteration, bool isLoop)
        {
            var Input = Knots.Where(data => data.applyItems.position == true).Select(data => data.position).ToArray();
            Debug.Log(Input.Length);
            CalcSpace cSpace = new KCurves.CalcSpace(Input.Length, isLoop);
            return CalcBezierControls(Input, cSpace, iteration, isLoop);
        }

        private static BezierControls CalcBezierControls(Vector3[] points, CalcSpace space, int iteration, bool isLoop)
        {
            if (points.Length != space.N)
            {
                throw new ArgumentException($"The length of {nameof(points)} must equals to {nameof(space)}.{nameof(space.N)}.");
            }
            if (points.Length == 0)
            {
                for (int i = 0; i < 3; i++)
                    space.C.Points[i] = Vector3.zero;
                return space.C;
            }
            if (points.Length == 1)
            {
                for (int i = 0; i < 3; i++)
                    space.C.Points[i] = points[0];
                return space.C;
            }
            if (points.Length == 2)
            {
                space.C.Points[0] = points[0];
                space.C.Points[1] = (points[0] + points[1]) / 2;
                space.C.Points[2] = points[1];
                return space.C;
            }

            Step0(points, space.C, space.L, space.A, isLoop);
            for (int i = 0; i < iteration; i++)
            {
                if (i < 3 || i < iteration / 2)
                Step1(space.C, space.L, isLoop);
                Step2(space.C, space.L);
                Step3(points, space.C, space.T);
                Step4(points, space.C, space.L, space.T, space.A, isLoop);
            }
            Step2(space.C, space.L);

            //input.Lengthとtの数は同じ
            SetTs(space.C,space.T, isLoop);

            return space.C;
        }

        static void SetTs(BezierControls cs, double[] spaceT,bool isLoop)
        {
            double[] ts = new double[spaceT.Length];
            ts[0]= isLoop? spaceT[0] : 0d;

            for (int i = 1; i < ts.Length - 1; i++)
            {
                ts[i] += spaceT[i];
            }
            ts[ts.Length - 1] = isLoop ? spaceT[ts.Length - 1] : ts.Length - 1;
            cs.Ts = ts;
        }

        static void Step0(Vector3[] ps, BezierControls cs, float[] lambdas, double[] A, bool isLoop)
        {
            var n = ps.Length;

            //全てのλを0.5で初期化
            for (var i = 0; i < n; i++)
                lambdas[i] = 0.5f;

            //ループしない場合、最初と最後から２番目を0,1に変更（最後はそもそも使わない）
            if (!isLoop)
            {
                lambdas[0] = 0;
                lambdas[n - 2] = 1;
                //lambdas[n - 1] = undefined;
            }

            //中央のベジェ制御点を全てユーザ制御点で初期化
            for (var i = 0; i < n; i++)
                cs[i, 1] = ps[i];

            //他のベジェ制御点を初期化
            for (var i = 0; i < n; i++)
            {
                var next = (i + 1) % n;
                cs[next, 0] = cs[i, 2] = (1 - lambdas[i]) * cs[i, 1] + lambdas[i] * cs[next, 1];
            }

            //行列の端の値は固定
            A[0] = 0;
            A[1] = 1;
            A[2] = 0;
            A[A.Length - 1] = 0;
            A[A.Length - 2] = 1;
            A[A.Length - 3] = 0;
            if (!isLoop)
            {
                //非ループの場合はさらにもう一行ずつ固定
                A[3] = 0;
                A[4] = 1;
                A[5] = 0;
                A[A.Length - 4] = 0;
                A[A.Length - 5] = 1;
                A[A.Length - 6] = 0;
            }
        }
        static void Step1(BezierControls cs, float[] lambdas, bool isLoop)
        {
            //三角形の面積を求める関数
            float TriArea(Vector3 p1, Vector3 p2, Vector3 p3)
            {
                p1 -= p3; p2 -= p3;
                return Mathf.Abs(p1.x * p2.y - p2.x * p1.y) / 2f;
            }

            var n = lambdas.Length;
            int begin = isLoop ? 0 : 1;
            int end = isLoop ? n : n - 2;
            for (var i = begin; i < end; i++)
            {
                var next = (i + 1) % n;
                var c = cs.Points;
                var t1 = TriArea(c[i * 2], c[i * 2 + 1], c[next * 2 + 1]);
                var t2 = TriArea(c[i * 2 + 1], c[next * 2 + 1], c[next * 2 + 2]);
                if (Mathf.Abs(t1 - t2) < 0.001f)
                    lambdas[i] = 0.5f;
                else
                    lambdas[i] = (t1 - Mathf.Sqrt(t1 * t2)) / (t1 - t2);
            }
        }

        static void Step2(BezierControls cs, float[] lambdas)
        {
            var n = lambdas.Length;
            for (var i = 0; i < n - 1; i++)
            {
                cs[i + 1, 0] = (1 - lambdas[i]) * cs[i, 1] + lambdas[i] * cs[i + 1, 1];
            }
            cs[0, 0] = cs[n - 1, 2] = (1 - lambdas[n - 1]) * cs[n - 1, 1] + lambdas[n - 1] * cs[0, 1];
        }

        static void Step3(Vector3[] ps, BezierControls cs, double[] ts)
        {
            for (int i = 0; i < ts.Length; i++)
            {
                //セグメントが潰れている場合は不定解なので0.5とする
                if (cs[i, 0] == cs[i, 2]) { ts[i] = 0.5; continue; }
                //セグメントの端にユーザ制御点がある場合は図形的に自明
                //TODO Vector3
                if (ps[i] == cs[i, 0]) { ts[i] = 0; continue; }
                if (ps[i] == cs[i, 2]) { ts[i] = 1; continue; }

                var c2 = cs[i, 2] - cs[i, 0];   // != 0
                var p = ps[i] - cs[i, 0];       // != 0

                double a = c2.sqrMagnitude;             // != 0
                double b = -3 * Vector3.Dot(c2, p);
                double c = Vector3.Dot(2 * p + c2, p);
                double d = -p.sqrMagnitude;             // != 0

                ts[i] = SolveCubicEquation(a, b, c, d);
            }
        }

        static void Step4(Vector3[] ps, BezierControls cs, float[] lambdas, double[] ts, double[] A, bool isLoop)
        {
            var n = ps.Length;

            //係数行列Aを構成（端の部分はStep0で初期化済）
            {
                for (int i = isLoop ? 0 : 1; i < (isLoop ? n : (n - 1)); i++)
                {
                    var ofs = (i + 1) * 3;
                    var next = (i + 1) % n;
                    var prev = (i - 1 + n) % n;

                    //ランクが下がってしまう場合微調整
                    if (ts[i] == 1 && ts[next] == 0 || !isLoop && i == n - 2 && ts[i] == 1)
                        ts[i] = 0.99999f;
                    if (!isLoop && i == 1 && ts[i] == 0)
                        ts[i] = 0.00001f;


                    var tmp = (1 - ts[i]) * (1 - ts[i]);
                    A[ofs] = (1 - lambdas[prev]) * tmp;
                    A[ofs + 1] = lambdas[prev] * tmp + (2 - (1 + lambdas[i]) * ts[i]) * ts[i];
                    A[ofs + 2] = lambdas[i] * ts[i] * ts[i];
                }
            }

            //入出力ベクトルを拡張
            var extendedPs = new ExtendedKnots(ps, cs);
            var extendedCs = new ExtendedBezierControls(cs);

            //連立方程式を解く
            SolveTridiagonalEquation(A, extendedCs, extendedPs);
        }

        static void SolveTridiagonalEquation(double[] A, ExtendedBezierControls x, ExtendedKnots b)
        {
            var n = A.Length / 3 - 2;

            /* A=LU */
            for (int i = 0, i3 = 0; i < n + 1; i++, i3 += 3)
            {
                A[i3 + 3] /= A[i3 + 1];                 //l21  := a21/a11
                A[i3 + 4] -= A[i3 + 3] * A[i3 + 2];     //a'11 := a22-l21u12
            }

            /* Ly=b */
            x[0] = b[0];                    //対角要素は全て1なので、最上行はそのまま            
            for (var i = 1; i < n + 1; i++) //対角要素の左隣の要素を対応するx（計算済み）にかけて引く
            {
                x[i] = b[i] - (float)A[i * 3] * x[i - 1];
            }

            /* Ux=y */
            x[n + 1] /= (float)A[(n + 1) * 3 + 1];              //最下行はただ割るだけ
            for (int i = n, i3 = n * 3; i >= 0; i--, i3 -= 3)   //対角要素の右隣の要素を対応するx（計算済み）にかけて引いて割る
            {
                x[i] = (x[i] - (float)A[i3 + 2] * x[i + 1]) / (float)A[i3 + 1];
            }
        }

        static double SolveCubicEquation(double a, double b, double c, double d)
        {
            //負の値に対応した３乗根
            double Cbrt(double x) => Math.Sign(x) * Math.Pow(Math.Abs(x), 1.0 / 3);

            b /= a * 3;
            c /= a;
            d /= a;
            var p = c / 3 - b * b;
            var q = b * b * b - (b * c - d) / 2;
            var D = q * q + p * p * p;

            if (Math.Abs(D) < 1.0E-12) //D = 0
            {
                var ret = Cbrt(q) - b;
                if (ret >= 0)
                    return Math.Min(ret, 1);
                else
                    return Math.Min(ret * -2, 1);
            }
            else if (D > 0)
            {
                var sqrtD = Math.Sqrt(D);
                var u = Cbrt(-q + sqrtD);
                var v = Cbrt(-q - sqrtD);
                var ret = u + v - b;
                return ret < 0 ? 0 : ret > 1 ? 1 : ret;
            }
            else //D < 0
            {
                var tmp = 2 * Math.Sqrt(-p);
                var arg = Math.Atan2(Math.Sqrt(-D), -q) / 3;
                const double pi2d3 = 2 * Math.PI / 3;
                var ret1 = tmp * Math.Cos(arg) - b;
                if (0 <= ret1 && ret1 <= 1) return ret1;
                var ret2 = tmp * Math.Cos(arg + pi2d3) - b;
                if (0 <= ret2 && ret2 <= 1) return ret2;
                var ret3 = tmp * Math.Cos(arg - pi2d3) - b;
                if (0 <= ret3 && ret3 <= 1) return ret3;
                throw new Exception($"Invalid solution: {ret1}, {ret2}, {ret3}");
            }
        }

        public class CalcSpace
        {
            public int N { get; private set; }              //制御点数
            internal float[] L { get; private set; }        //λ
            internal ExtendBezierControls C { get; private set; } //ベジェ制御点（出力）
            internal double[] T { get; private set; }       //t
            internal double[] A { get; private set; }       //Step4の行列計算用メモリ

            public CalcSpace(int n, bool isLoop)
            {
                N = n;
                L = new float[n];
                C = new ExtendBezierControls(n, isLoop);
                T = new double[n];
                A = new double[(n + 2) * 3];
            }
            public BezierControls Result => C;
        }
        struct ExtendedKnots
        {
            Vector3 top;
            Vector3[] ps;
            Vector3 bottom;

            public Vector3 this[int i]
            {
                get => i == 0 ? top : i <= ps.Length ? ps[i - 1] : bottom;
                set
                {
                    if (i == 0) top = value;
                    else if (i <= ps.Length) ps[i - 1] = value;
                    else bottom = value;
                }
            }

            public ExtendedKnots(Vector3[] ps, BezierControls cs)
            {
                top = cs[cs.SegmentCount - 1, 1];
                this.ps = ps;
                bottom = cs[0, 1];
            }
        }



        struct ExtendedBezierControls
        {
            Vector3 top;
            Vector3[] cs;
            Vector3 bottom;

            public Vector3 this[int i]
            {
                get => i == 0 ? top : i <= cs.Length / 2 ? cs[i * 2 - 1] : bottom;
                set
                {
                    if (i == 0) top = value;
                    else if (i <= cs.Length / 2) cs[i * 2 - 1] = value;
                    else bottom = value;
                }
            }

            public ExtendedBezierControls(BezierControls cs)
            {
                top = cs[cs.SegmentCount - 1, 1];
                this.cs = cs.Points;
                bottom = cs[0, 1];
            }
        }
    }
}
