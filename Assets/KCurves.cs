using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public static class KCurves
    {
        //�C�e���[�V������
        private static readonly int iteration = 10;

        private static CalcSpace cSpace;

        public static BezierControls CalcBezier(List<ControlPoint> Knots, bool isLoop)
        {
            var Input = Knots.Where(data => data.applyItems.position == true).Select(data => data.position).ToArray();
            Debug.Log(Input.Length);
            cSpace = new KCurves.CalcSpace(Input.Length, isLoop);
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

            //input.Length��t�̐��͓���
            SetTs(space.C,space.T);

            return space.C;
        }

        static void SetTs(BezierControls cs, double[] spaceT)
        {
            double[] ts = new double[spaceT.Length];
            ts[0]= 0d;

            for (int i = 1; i < ts.Length - 1; i++)
            {
                ts[i] += spaceT[i];
            }
            ts[ts.Length - 1] = ts.Length - 1;
            cs.Ts = ts;
        }

        static void Step0(Vector3[] ps, BezierControls cs, float[] lambdas, double[] A, bool isLoop)
        {
            var n = ps.Length;

            //�S�Ẵɂ�0.5�ŏ�����
            for (var i = 0; i < n; i++)
                lambdas[i] = 0.5f;

            //���[�v���Ȃ��ꍇ�A�ŏ��ƍŌォ��Q�Ԗڂ�0,1�ɕύX�i�Ō�͂��������g��Ȃ��j
            if (!isLoop)
            {
                lambdas[0] = 0;
                lambdas[n - 2] = 1;
                //lambdas[n - 1] = undefined;
            }

            //�����̃x�W�F����_��S�ă��[�U����_�ŏ�����
            for (var i = 0; i < n; i++)
                cs[i, 1] = ps[i];

            //���̃x�W�F����_��������
            for (var i = 0; i < n; i++)
            {
                var next = (i + 1) % n;
                cs[next, 0] = cs[i, 2] = (1 - lambdas[i]) * cs[i, 1] + lambdas[i] * cs[next, 1];
            }

            //�s��̒[�̒l�͌Œ�
            A[0] = 0;
            A[1] = 1;
            A[2] = 0;
            A[A.Length - 1] = 0;
            A[A.Length - 2] = 1;
            A[A.Length - 3] = 0;
            if (!isLoop)
            {
                //�񃋁[�v�̏ꍇ�͂���ɂ�����s���Œ�
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
            //�O�p�`�̖ʐς����߂�֐�
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
                //�Z�O�����g���ׂ�Ă���ꍇ�͕s����Ȃ̂�0.5�Ƃ���
                if (cs[i, 0] == cs[i, 2]) { ts[i] = 0.5; continue; }
                //�Z�O�����g�̒[�Ƀ��[�U����_������ꍇ�͐}�`�I�Ɏ���
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

            //�W���s��A���\���i�[�̕�����Step0�ŏ������ρj
            {
                for (int i = isLoop ? 0 : 1; i < (isLoop ? n : (n - 1)); i++)
                {
                    var ofs = (i + 1) * 3;
                    var next = (i + 1) % n;
                    var prev = (i - 1 + n) % n;

                    //�����N���������Ă��܂��ꍇ������
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

            //���o�̓x�N�g�����g��
            var extendedPs = new ExtendedPlayerControls(ps, cs);
            var extendedCs = new ExtendedBezierControls(cs);

            //�A��������������
            SolveTridiagonalEquation(A, extendedCs, extendedPs);
        }
        static void SolveTridiagonalEquation(double[] A, ExtendedBezierControls x, ExtendedPlayerControls b)
        {
            var n = A.Length / 3 - 2;

            /* A=LU */
            for (int i = 0, i3 = 0; i < n + 1; i++, i3 += 3)
            {
                A[i3 + 3] /= A[i3 + 1];                 //l21  := a21/a11
                A[i3 + 4] -= A[i3 + 3] * A[i3 + 2];     //a'11 := a22-l21u12
            }

            /* Ly=b */
            x[0] = b[0];                    //�Ίp�v�f�͑S��1�Ȃ̂ŁA�ŏ�s�͂��̂܂�            
            for (var i = 1; i < n + 1; i++) //�Ίp�v�f�̍��ׂ̗v�f��Ή�����x�i�v�Z�ς݁j�ɂ����Ĉ���
            {
                x[i] = b[i] - (float)A[i * 3] * x[i - 1];
            }

            /* Ux=y */
            x[n + 1] /= (float)A[(n + 1) * 3 + 1];              //�ŉ��s�͂������邾��
            for (int i = n, i3 = n * 3; i >= 0; i--, i3 -= 3)   //�Ίp�v�f�̉E�ׂ̗v�f��Ή�����x�i�v�Z�ς݁j�ɂ����Ĉ����Ċ���
            {
                x[i] = (x[i] - (float)A[i3 + 2] * x[i + 1]) / (float)A[i3 + 1];
            }
        }
        static double SolveCubicEquation(double a, double b, double c, double d)
        {
            //���̒l�ɑΉ������R�捪
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
            public int N { get; private set; }              //����_��
            internal float[] L { get; private set; }        //��
            internal ExtendBezierControls C { get; private set; } //�x�W�F����_�i�o�́j
            internal double[] T { get; private set; }       //t
            internal double[] A { get; private set; }       //Step4�̍s��v�Z�p������

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
    }
}
