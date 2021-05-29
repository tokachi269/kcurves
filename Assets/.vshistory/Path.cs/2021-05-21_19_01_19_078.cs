using Assets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Path : MonoBehaviour
    {
        public string Name { get; private set; }
        //���[�U�[����_
        public List<ControlPoint> Knots = new List<ControlPoint>();
        //���[�U�[���䒍���_
        public List<ControlPoint> LookAts = new List<ControlPoint>();
        //Bezier�v�Z����
        public ExtendBezierControls Beziers;
        public bool isLoop { get; private set; }

        public void SetBezierFromKnots()
        {
            Beziers = KCurves.CalcBezier( Knots, isLoop) as ExtendBezierControls;
            // plots���v�Z
            //extendBezierControls.CalcPlots(step, isLoop, isEquallySpaced);
            //bezierResult.CalcKnotsLength(isLoop);
            //extendBezierControls.CalcTotalKnotsLength(isLoop);

            //t�̈ړ��������v�Z���A�p�����[�^������
            //extendBezierControls.CalcArcLengthWithT(isLoop);
        }

        // ���[�U�[����_���x�W�F����_�ɒǉ�����
        public Vector3[] DividePoints(bool isLoop, out int dividedSegmentCount)
        {                                                                                 
            dividedSegmentCount = (Beziers.SegmentCount - 2 ) * 2;
            Vector3[] dividedPoints = new Vector3[dividedSegmentCount * 2 + 1];
            int segCnt = isLoop || Beziers.SegmentCount < 3 ? Beziers.SegmentCount : Beziers.SegmentCount - 2;

            for (int i = 1; i <= segCnt; i++)
            {
                Debug.Log("i=" + i +","+ Beziers[i, 0] +","+ Beziers[i, 1] + "," + Beziers[i, 2] + "," + (float)Beziers.Ts[i]);
                Vector3[] result = BezierUtil.Divide(Beziers[i, 0], Beziers[i, 1], Beziers[i, 2], (float)Beziers.Ts[i]);

                for (int j = 0; j <= 3; j++)
                {
                    int index = 4 * (i - 1) + j;
                    Debug.Log(index + ", "+result[j]);
                    if(index != 0 && (index + 2) % 4 == 0)
                    {
                        dividedPoints[index] = Knots[(index + 2) / 4].position;
                    }
                    else
                    {
                        dividedPoints[index] = result[j];
                    }
                }
            }
            dividedPoints[dividedPoints.Length-1] = Knots[Knots.Count-1].position;
            Debug.Log(dividedPoints.Length - 1 + ", " + dividedPoints[dividedPoints.Length - 1]);
            return dividedPoints;
        }

        public Vector3[] Output(int step, bool isLoop)
        {
            if (Beziers is null) {
                SetBezierFromKnots();
            }
            if (Beziers.SegmentCount >= 3 && (Beziers.SegmentCount == (Knots.Count < 3 ? 1 : Knots.Count)))
            {
                int segment;
                Vector3[] divVec= DividePoints(isLoop, out segment);
                Beziers = new ExtendBezierControls(segment, divVec, isLoop);
            }

            Debug.Log("x segCnt"+Beziers.SegmentCount);
            Debug.Log("x length"+Beziers.Points.Length);
            return Beziers.CalcPlots(Knots.Count, step, isLoop);
        }


        public Vector3 CalcPosition(bool isLoop, float t)
        {            
            int segIndex = (int)Math.Truncate(t % Beziers.SegmentCount);
            if (segIndex > Beziers.SegmentCount)
            {
                segIndex = (isLoop || Beziers.SegmentCount < 3) ? 1 : segIndex++;
            }
            return BezierUtil.Position(Beziers[segIndex, 0], Beziers[segIndex, 1], Beziers[segIndex, 2], t % 1);
        }


        public Quaternion CalcRotation(int segIndex, float inputL)
        {

            int knotIndex, knotNextIndex;
            float t,sum = 0;
            if (segIndex == 0)
            {
                knotIndex = 0;
            }
            else if(segIndex == Beziers.SegmentCount - 1)
            {
                knotIndex = Beziers.SegmentCount - 1;
            }
            else
            {
                knotIndex = (segIndex + 1) / 3 + 1;
            }

            knotNextIndex = (isLoop || knotIndex < Knots.Count -1 ? knotIndex + 1 : knotIndex);

            //Knots�Ԃɑ��݂���bezier�̍��v�����߂�
            if (knotIndex == 0) 
            {
                sum += Beziers.Lengths[0, Beziers.ArcLengthWithTStep - 1]; 
            }
            else if (knotIndex == Beziers.SegmentCount - 1)
            {
                 sum += Beziers.Lengths[knotIndex, Beziers.ArcLengthWithTStep - 1];
            }
            else {
                for (int i = segIndex - 1; i <= segIndex + 1; i++)
                {
                    if (knotIndex == (segIndex + 1) / 3 + 1)
                    {
                        sum += Beziers.Lengths[i, Beziers.ArcLengthWithTStep - 1];
                    }
                }
            }
            t = inputL/sum;




            Debug.Log(knotIndex +", "+knotNextIndex);
            Vector3 rotation;

            if (!Quaternion.Equals(Knots[knotIndex].rotation, Knots[knotNextIndex].rotation))
            {
                Debug.Log("Knots[knotIndex].rotation.eulerAngles"+ Knots[knotIndex].rotation.eulerAngles);
                Debug.Log("Knots[knotNextIndex].rotation.eulerAngles" + Knots[knotNextIndex].rotation.eulerAngles);
                if (t <= 0f && segIndex == Beziers.SegmentCount - 1)
                {
                    rotation = Knots[0].rotation.eulerAngles;

                }
                else if (t >= 0.99999f && segIndex == Beziers.SegmentCount - 1)
                {
                    rotation = Knots[Knots.Count - 1].rotation.eulerAngles;

                }
                else
                {
                    rotation = Vector3.Lerp(Knots[knotIndex].rotation.eulerAngles, ClosestAngle(Knots[knotIndex].rotation.eulerAngles, Knots[knotNextIndex].rotation.eulerAngles), t);
                }
            }
            else
            {
                rotation = Knots[knotIndex].rotation.eulerAngles;
            }
            if (LookAts.Count != 0)
            {
                if (Knots[knotIndex].isLookAt || Knots[knotNextIndex].isLookAt)
                {
                    rotation = Vector3.Lerp((Knots[knotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation),
                                           (Knots[knotNextIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation), t);
                }
            }
            rotation.z = 0f;
            return Quaternion.Euler(rotation);
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
            if (!Beziers.IsCalcArcLengthWithT) Beziers.CalcArcLengthWithT(Knots.Count,isLoop);

            return Beziers.TotalLength / time;
        }

        /*P2��P1�����]�����Ȃ������ɉ�]����悤�ɕ␳����*/
        public static Vector3 ClosestAngle(Vector3 basePoint,  Vector3 point)
        {
            Vector3 A = basePoint - point;
            if (A.x > 180f)
            {
                point.x += 360f;
            }
            else if (A.x < -180f)
            {
                point.x -= 360f;
            }
            if (A.y > 180f)
            {
                point.y += 360f;
            }
            else if (A.y < -180f)
            {
                point.y -= 360f;
            }
            if (A.z > 180f)
            {
                point.z += 360f;
            }
            else if (A.z < -180f)
            {
                point.z -= 360f;
            }
            return point;
        }
    }
}