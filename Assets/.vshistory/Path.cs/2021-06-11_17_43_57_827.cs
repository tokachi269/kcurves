using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Path : MonoBehaviour
    {
        public string Name { get; private set; }

        //ユーザー制御点
        public List<ControlPoint> Knots { get; private set; } = new List<ControlPoint>();

        //ユーザー制御注視点
        public List<ControlPoint> LookAts { get; private set; } = new List<ControlPoint>();

        //Bezier計算結果
        public ExtendBezierControls Beziers { get; private set; }

        public bool isLoop { get; private set; }

        private float t = 0;
        private int bezierIndex = 0;
        private float ProgressLength = 0f;
        private float maxSpeed = 0f;

        public float dist = 0;
        public float distall = 0;
        public float diffT = 0;
        private Vector3 bef = Vector3.zero;
        private float befT = 0;

        public int time = 10;

        private EasingMode mode;
        public static GameObject moveCameraCube;

        public void Start()
        {
            moveCameraCube = new GameObject("moveCameraCube");
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.parent = this.transform;
        }

        public void SetBezierFromKnots()
        {
            Beziers = KCurves.CalcBezier(Knots, isLoop) as ExtendBezierControls;
        }

        public IEnumerator MoveCamera()
        {
            for (float currentTime= 0; ;)
            {
                //Debug.Log("TotalLength:" + path.Beziers.TotalLength);

                maxSpeed = MaxSpeed(time);
                //Debug.Log("maxSpeed" + maxSpeed);

                int knotIndex = bezierIndex % 2 == 0 ? (bezierIndex / 2) : (bezierIndex + 1) / 2;
                mode = EasingMode.None;
                //mode = (EasingMode)((byte)Knots[knotIndex].easingMode | ((byte)Knots[knotIndex + 1].easingMode << 1));
                Debug.Log("bezierIndex" + bezierIndex);
                Debug.Log("knotIndex" + knotIndex);
                if (knotIndex == 5)
                    Debug.Log(5);
                float KnotBetweenRange = 0;

                if (knotIndex == 0 || knotIndex == Knots.Count - 1)
                {
                    KnotBetweenRange = Beziers.Length(bezierIndex);
                }
                else
                {
                    for (ushort j = (ushort)(2 * knotIndex - 1); j < Beziers.SegmentCount && j <= 2 * knotIndex; j++)
                    {
                        KnotBetweenRange += Beziers.Length(j);
                    }
                }
                Debug.Log(KnotBetweenRange);

                bool isSegChanged = false;
                if (bezierIndex == 0 || bezierIndex == Beziers.SegmentCount)
                {
                    if (ProgressLength >= Beziers.Length(bezierIndex))
                    {
                        ProgressLength -= Beziers.Length(bezierIndex);
                        isSegChanged = true;
                    }
                }
                else if (bezierIndex % 2 == 1)
                {
                    if (ProgressLength >= Beziers.Length(bezierIndex))
                    {
                        isSegChanged = true;
                    }
                }
                else
                {
                    if (ProgressLength >= Beziers.Length(bezierIndex - 1) + Beziers.Length(bezierIndex))
                    {
                        ProgressLength -= Beziers.Length(bezierIndex - 1) + Beziers.Length(bezierIndex);
                        isSegChanged = true;
                    }
                }
                if (isSegChanged)
                {
                    bezierIndex++;
                    Debug.Log("knotIndex++");
                    if (bezierIndex == Beziers.SegmentCount)
                    {
                        bezierIndex = 0;
                        t = 0f;
                        ProgressLength = 0f;
                        yield break;
                    }
                }


                float easing = Easing.GetEasing(mode, ProgressLength, KnotBetweenRange);

                //Debug.Log("easing:"+easing);

                if (bezierIndex != 0 && bezierIndex % 2 == 0)
                {
                    easing -= Beziers.Length(2 * knotIndex - 1);
                }

                t = Beziers.GetT(bezierIndex, easing);

                // Debug.Log("t:"+ t);
                //Debug.Log("bezierIndex:" + bezierIndex + "  ProgressLength:" + ProgressLength + "maxS:" + maxSpeed + "currentTime:" + currentTime);
                //{
                //    diffT = t - befT;
                //    //Debug.Log("t:" + diffT);
                //    befT = t;
                //    Vector3 now = CalcPosition(isLoop, t);
                //
                //    distall += dist;
                //    dist = Vector3.Distance(bef, now);
                //    bef = now;
                //    //Debug.Log("dist:" + dist);
                //}

                if (t <= Beziers.SegmentCount)
                {
                    Vector3 pos = CalcPosition(bezierIndex, t, isLoop);
                    //Quaternion rot = CalcRotation(bezierIndex, easing);
                    moveCameraCube.transform.position = pos;
                    //moveCameraCube.transform.rotation = rot;
                    //GameObject.Find("CameraRig").transform.position = pos;
                    //GameObject.Find("CameraRig").transform.rotation = rot;
                }

                float dt = Time.deltaTime;

                ProgressLength += maxSpeed * dt;
                currentTime += dt;
                yield return null;
            }
        }

        public Vector3 CalcPosition(int bezierIndex, float t, bool isLoop)
        {

            return BezierUtil.Position(Beziers[bezierIndex, 0], Beziers[bezierIndex, 1], Beziers[bezierIndex, 2], t % 1);
        }

        public Quaternion CalcRotation(int segIndex, float inputL)
        {
            ushort knotIndex, nextKnotIndex;
            GetBeforeAndBehindKnot(segIndex, out knotIndex, out nextKnotIndex);
            float t, sum = 0;

            //Knots間に存在するbezierの合計を求める
            if (knotIndex == 0)
            {
                sum += Beziers.Lengths[0, Beziers.ArcLengthWithTStep - 1];
            }
            else if (knotIndex == Beziers.SegmentCount - 1)
            {
                sum += Beziers.Lengths[knotIndex, Beziers.ArcLengthWithTStep - 1];
            }
            else
            {
                for (ushort i = (ushort)(segIndex - 1); i <= segIndex + 1; i++)
                {
                    if (knotIndex == (segIndex + 1) / 3 + 1)
                    {
                        sum += Beziers.Lengths[i, Beziers.ArcLengthWithTStep - 1];
                    }
                }
            }

            t = inputL / sum;

            Vector3 rotation;

            if (!Quaternion.Equals(Knots[knotIndex].rotation, Knots[nextKnotIndex].rotation))
            {
                Debug.Log("Knots[knotIndex].rotation.eulerAngles" + Knots[knotIndex].rotation.eulerAngles);
                Debug.Log("Knots[knotNextIndex].rotation.eulerAngles" + Knots[nextKnotIndex].rotation.eulerAngles);
                if (t <= 0f && segIndex == 0)
                {
                    rotation = Knots[0].rotation.eulerAngles;
                }
                else if (t >= 0.99999f && segIndex == Beziers.SegmentCount - 1)
                {
                    rotation = Knots[Knots.Count - 1].rotation.eulerAngles;
                }
                else
                {
                    rotation = Vector3.Lerp(Knots[knotIndex].rotation.eulerAngles, ClosestAngle(Knots[knotIndex].rotation.eulerAngles, Knots[nextKnotIndex].rotation.eulerAngles), t);
                }
            }
            else
            {
                rotation = Knots[knotIndex].rotation.eulerAngles;
            }
            if (LookAts.Count != 0)
            {
                if (Knots[knotIndex].isLookAt || Knots[nextKnotIndex].isLookAt)
                {
                    rotation = Vector3.Lerp((Knots[knotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation),
                                           (Knots[nextKnotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation), t);
                }
            }
            rotation.z = 0f;
            return Quaternion.Euler(rotation);
        }

        // ユーザー制御点をベジェ制御点に追加する
        public Vector3[] DividePoints(bool isLoop, out int dividedSegmentCount)
        {
            //Debug.Log("Beziers.SegmentCount:" + Beziers.SegmentCount);
            dividedSegmentCount = Beziers.SegmentCount < 3 ? 2 : (Beziers.SegmentCount - 2) * 2;
            //Debug.Log("dividedSegmentCount:" + dividedSegmentCount);
            Vector3[] dividedPoints = new Vector3[dividedSegmentCount * 2 + 1];
            ushort segCnt = (ushort)(isLoop || Beziers.SegmentCount < 3 ? Beziers.SegmentCount : Beziers.SegmentCount - 2);

            for (ushort i = 1; i <= segCnt; i++)
            {
                //Debug.Log("i=" + i +","+ Beziers[i, 0] +","+ Beziers[i, 1] + "," + Beziers[i, 2] + "," + (float)Beziers.Ts[i]);
                Vector3[] result = BezierUtil.Divide(Beziers[i, 0], Beziers[i, 1], Beziers[i, 2], (float)Beziers.Ts[i]);

                for (ushort j = 0; j <= 3; j++)
                {
                    ushort index = (ushort)(4 * (i - 1) + j);
                    //Debug.Log(index + ", "+result[j]);
                    if (index != 0 && (index + 2) % 4 == 0)
                    {
                        dividedPoints[index] = Knots[(index + 2) / 4].position;
                    }
                    else
                    {
                        dividedPoints[index] = result[j];
                    }
                }
            }
            dividedPoints[dividedPoints.Length - 1] = Knots[Knots.Count - 1].position;
            //Debug.Log(dividedPoints.Length - 1 + ", " + dividedPoints[dividedPoints.Length - 1]);
            return dividedPoints;
        }

        public Vector3[] Output(ushort step, bool isLoop)
        {
            if (Beziers is null)
            {
                SetBezierFromKnots();
            }
            if (Beziers.SegmentCount >= 3 && (Beziers.SegmentCount == (Knots.Count < 3 ? 1 : Knots.Count)))
            {
                int segment;
                Vector3[] divVec = DividePoints(isLoop, out segment);
                Beziers = new ExtendBezierControls(segment, divVec, isLoop);
            }

            Debug.Log("x segCnt" + Beziers.SegmentCount);
            Debug.Log("x length" + Beziers.Points.Length);
            return Beziers.CalcPlots(step, isLoop);
        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov, bool lookAt)
        {
            this.Knots.Add(new ControlPoint(position, rotation, fov, lookAt));
            if (Knots.Count == 0)
            {
                moveCameraCube.transform.position = Knots[0].position;
            }
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

        public float MaxSpeed(int time)
        {
            if (!Beziers.IsCalcArcLengthWithT) Beziers.CalcArcLengthWithT(isLoop);

            return Beziers.TotalLength / time;
        }

        /*P2をP1から回転が少ない方向に回転するように補正する*/

        public static Vector3 ClosestAngle(Vector3 basePoint, Vector3 point)
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

        public void GetBeforeAndBehindKnot(int segIndex, out ushort knotIndex, out ushort nextKnotIndex)
        {
            if (segIndex == 0)
            {
                knotIndex = 0;
            }
            else if (segIndex == Beziers.SegmentCount - 1)
            {
                knotIndex = (ushort)(Beziers.SegmentCount - 1);
            }
            else
            {
                knotIndex = (ushort)((segIndex + 1) / 3 + 1);
            }

            nextKnotIndex = ((ushort)(isLoop || knotIndex < Knots.Count - 1 ? knotIndex + 1 : knotIndex));
        }
    }
}