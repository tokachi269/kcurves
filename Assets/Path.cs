using Assets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Path
    {
        public string name { get; private set; }
        //���[�U�[����_
        public List<ControlPoint> Knots = new List<ControlPoint>();
        //���[�U�[���䒍���_
        public List<ControlPoint> LookAts = new List<ControlPoint>();
        //Bezier�v�Z����
        public ExtendBezierControls extendBezierControls;
        //�p�����[�^t�̌ʒ�
        public float[,] Lengths { get; internal set; }
        public float TotalLength { get; private set; }
        public int ArcLengthWithTStep { get; private set; } = 10;

        public bool IsCalcArcLengthWithT { get; private set; }
        public bool isLoop { get; private set; }

        public void SetBezierFromKnots()
        {
            //TODO �R���X�g���N�^��cast����悤�ɕύX
            extendBezierControls = (ExtendBezierControls)KCurves.CalcBezier( Knots, isLoop);

            if (!(extendBezierControls is null))
            {
                extendBezierControls.CalcArcLengthWithT(isLoop);
            }
            // plots���v�Z
            //extendBezierControls.CalcPlots(step, isLoop, isEquallySpaced);
            //bezierResult.CalcKnotsLength(isLoop);
            //extendBezierControls.CalcTotalKnotsLength(isLoop);

            //t�̈ړ��������v�Z���A�p�����[�^������
            //extendBezierControls.CalcArcLengthWithT(isLoop);

        }

        public Vector3[] Output(int step, bool isLoop)
        {

                SetBezierFromKnots();
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
            float t = inputL / Lengths[segIndex, ArcLengthWithTStep - 1];
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


    }
}