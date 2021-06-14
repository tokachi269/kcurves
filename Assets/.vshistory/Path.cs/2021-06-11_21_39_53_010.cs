using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public interface ICameraMode{
        string Name{ get; }
        string time { get; }
        List<ControlPoint> Knots { get; }
        IEnumerator MoveCamera();
        void AddLookAt(Vector3 position, Quaternion rotation, float fov);
        void RemoveKnot();
        void Render();

    }

    public class Path : MonoBehaviour, ICameraMode
    {
        public string Name { get; private set; }

        //ユーザー制御点
        public List<ControlPoint> Knots { get; private set; } = new List<ControlPoint>();

        //ユーザー制御注視点
        public List<ControlPoint> LookAts { get; private set; } = new List<ControlPoint>();

        //Bezier計算結果
        public ExtendBezierControls Beziers { get; private set; }

        public bool isLoop { get; private set; }


        public float dist = 0;
        public float distall = 0;
        public float diffT = 0;
        private Vector3 bef = Vector3.zero;
        private float befT = 0;

        public int time = { get; private set; }

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
            float maxSpeed = MaxSpeed(time);
            int knotIndex = 0;
            int bezierIndex = 0;

            float ProgressLength = 0f;
            var easingMode = SetEasingMode();


            float KnotBetweenRange = Beziers.Length(0);
            if (Knots[0].delay != 0f)
            {
                yield return new WaitForSeconds(Knots[0].delay);
            }

            EasingMode mode = (EasingMode)((byte)easingMode[1] | ((byte)easingMode[0] << 1));

            for (float currentTime= 0; ;)
            {
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

                    knotIndex = bezierIndex % 2 == 0 ? (bezierIndex / 2) : (bezierIndex + 1) / 2;
                    //mode = EasingMode.None;
                    mode = (EasingMode)((byte)easingMode[knotIndex + 1] | ((byte)easingMode[knotIndex] << 1));

                    if (knotIndex == Knots.Count - 1)
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


                    if (Knots[knotIndex].delay != 0f)
                    {
                        yield return new WaitForSeconds(Knots[knotIndex].delay);
                    }
                    Debug.Log("knotIndex++");
                    if (bezierIndex == Beziers.SegmentCount)
                    {
                        yield break;
                    }
                }

                Debug.Log("mode:" + mode + " bezierIndex:" + bezierIndex + " knotIndex:" + knotIndex + " KnotBetweenRange:" + KnotBetweenRange);

                float easing = Easing.GetEasing(mode, ProgressLength, KnotBetweenRange);

                //Debug.Log("easing:"+easing);

                if (bezierIndex != 0 && bezierIndex % 2 == 0)
                {
                    easing -= Beziers.Length(2 * knotIndex - 1);
                }

                float t = Beziers.GetT(bezierIndex, easing);

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
                    Quaternion rot = CalcRotation(knotIndex, easing);
                    moveCameraCube.transform.position = pos;
                    moveCameraCube.transform.rotation = rot;
                    //GameObject.Find("CameraRig").transform.position = pos;
                    //GameObject.Find("CameraRig").transform.rotation = rot;
                }

                float dt = Time.deltaTime;

                ProgressLength += maxSpeed * dt;
                currentTime += dt;
                yield return null;
            }
        }

        private EasingMode[] SetEasingMode()
        {
            var list = Knots.Select(x => x.easingMode).ToArray();
            if (list[0] == EasingMode.Auto)
            {
                list[0] = EasingMode.None;
            }
            if (list[list.Length - 1] == EasingMode.Auto)
            {
                list[list.Length - 1] = EasingMode.None;
            }

            return list;
        }

        public Vector3 CalcPosition(int bezierIndex, float t, bool isLoop)
        {

            return BezierUtil.Position(Beziers[bezierIndex, 0], Beziers[bezierIndex, 1], Beziers[bezierIndex, 2], t % 1);
        }

        public Quaternion CalcRotation(int knotIndex, float ratio)
        {
            Vector3 rotation;

            if (!Quaternion.Equals(Knots[knotIndex].rotation, Knots[knotIndex+1].rotation))
            {
                Debug.Log("Knots[knotIndex].rotation.eulerAngles" + Knots[knotIndex].rotation.eulerAngles);
                Debug.Log("Knots[knotNextIndex].rotation.eulerAngles" + Knots[knotIndex+1].rotation.eulerAngles);
                if (ratio <= 0f && knotIndex == 0)
                {
                    rotation = Knots[0].rotation.eulerAngles;
                }
                else if (ratio >= 0.99999f && knotIndex == Beziers.SegmentCount - 1)
                {
                    rotation = Knots[Knots.Count - 1].rotation.eulerAngles;
                }
                else
                {
                    rotation = Vector3.Lerp(Knots[knotIndex].rotation.eulerAngles, BezierUtil.ClosestAngle(Knots[knotIndex].rotation.eulerAngles, Knots[knotIndex+1].rotation.eulerAngles), ratio);
                }
            }
            else
            {
                rotation = Knots[knotIndex].rotation.eulerAngles;
            }


            if (LookAts.Count != 0)
            {
                if (Knots[knotIndex].isLookAt || Knots[knotIndex+1].isLookAt)
                {
                    rotation = Vector3.Lerp((Knots[knotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation),
                                           (Knots[knotIndex+1].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation), ratio);
                }
            }

            rotation.z = 0f;
            return Quaternion.Euler(rotation);
        }

        // ユーザー制御点をベジェ制御点に追加する
        private Vector3[] DividePoints(bool isLoop, out int dividedSegmentCount)
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

        private float MaxSpeed(int time)
        {
            if (!Beziers.IsCalcArcLengthWithT) Beziers.CalcArcLengthWithT(isLoop);

            return Beziers.TotalLength / time;
        }


        public void Render()
        {
            throw new NotImplementedException();
        }
    }
}