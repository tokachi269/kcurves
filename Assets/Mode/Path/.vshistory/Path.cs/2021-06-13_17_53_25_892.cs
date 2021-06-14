using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Path
{

    public class Path : BaseCameraMode, ICameraMode
    {
        public override string Name { get; set; }

        //ユーザー制御点
        protected override List<ControlPoint> Knots { get; set; } = new List<ControlPoint>();
        public int KnotsCount => Knots.Count;

        //ユーザー制御注視点
        protected List<ControlPoint> LookAts { get; private set; } = new List<ControlPoint>();
        public int LookAtsCount => Knots.Count;

        //Bezier計算結果
        protected ExtendBezierControls Beziers { get; private set; }
        public int BeziersCount => Beziers.SegmentCount;
        public int BeziersPointsLength => Beziers.Points.Length;
        public float TotalLength => Beziers.TotalLength;

        public bool IsLoop { get; private set; }
        public ushort Time { get; set; }
        public ushort step = 10;
        protected ControlPoint DefaultCameraPosition { get; set; }

        private static GameObject moveCameraCube;

        public static List<GameObject> inputCube = new List<GameObject>();
        public List<GameObject> bezierObject = new List<GameObject>();

        public static LineRenderer render;
        public float dist = 0;
        public float distall = 0;
        public float diffT = 0;
        private Vector3 bef = Vector3.zero;
        private float befT = 0;

        public void Start()
        {
            moveCameraCube = new GameObject("moveCameraCube");
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.parent = this.transform;
            render = gameObject.AddComponent<LineRenderer>();

        }

        public void SetBezierFromKnots()
        {
            Beziers = KCurves.CalcBezier(Knots, IsLoop) as ExtendBezierControls;
            Render();
        }

        public IEnumerator Play()
        {
            float maxSpeed = MaxSpeed(Time);
            int knotIndex = 0;
            int bezierIndex = 0;

            float BezierProgressLength = 0f;
            float KnotProgressLength = 0f;
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
                BezierProgressLength - BezierProgressLength;
                if (bezierIndex == 0 || bezierIndex == Beziers.SegmentCount)
                {
                    if (BezierProgressLength >= Beziers.Length(bezierIndex))
                    {
                        BezierProgressLength -= Beziers.Length(bezierIndex);
                        isSegChanged = true;
                    }
                }
                else if (bezierIndex % 2 == 1)
                {
                    if (BezierProgressLength >= Beziers.Length(bezierIndex))
                    {
                        isSegChanged = true;
                    }
                }
                else
                {
                    if (BezierProgressLength >= Beziers.Length(bezierIndex - 1) + Beziers.Length(bezierIndex))
                    {
                        BezierProgressLength -= Beziers.Length(bezierIndex - 1) + Beziers.Length(bezierIndex);
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

                Debug.Log("mode:" + mode + " bezierIndex:" + bezierIndex + " knotIndex:" + knotIndex + " KnotBetweenRange:" + KnotBetweenRange+ "ProgressLength" + BezierProgressLength);

                float easingLength = Easing.GetEasing(mode, BezierProgressLength, KnotBetweenRange);

                //Debug.Log("easing:"+ easing);

                float t = Beziers.GetT(bezierIndex, bezierIndex != 0 && bezierIndex % 2 == 0 ? easingLength - Beziers.Length(2 * knotIndex - 1) : easingLength);

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
                    Vector3 pos = CalcPosition(bezierIndex, t, IsLoop);

                    //TODO ProgressLengthがKnotBetweenRangeまで到達しない ProgressLengthはベジェ一つ分の長さだから
                    Quaternion rot = CalcRotation(knotIndex, easingLength / KnotBetweenRange);
                    moveCameraCube.transform.position = pos;
                    moveCameraCube.transform.rotation = rot;
                    //GameObject.Find("CameraRig").transform.position = pos;
                    //GameObject.Find("CameraRig").transform.rotation = rot;
                }

                float dt = UnityEngine.Time.deltaTime;

                KnotProgressLength += maxSpeed * dt;
                currentTime += dt;
                yield return null;
            }
        }

        private Vector3 CalcPosition(int bezierIndex, float t, bool isLoop)
        {

            return BezierUtil.Position(Beziers[bezierIndex, 0], Beziers[bezierIndex, 1], Beziers[bezierIndex, 2], t % 1);
        }

        private Quaternion CalcRotation(int knotIndex, float ratio)
        {
            Debug.Log(ratio);
            Vector3 rotation;

            if (!Quaternion.Equals(Knots[knotIndex].rotation, Knots[knotIndex+1].rotation))
            {
                Debug.Log("" + knotIndex + " Knots[knotIndex].rotation.eulerAngles:" + Knots[knotIndex].rotation.eulerAngles);
                Debug.Log("" + (knotIndex + 1) + " Knots[knotNextIndex].rotation.eulerAngles:" + Knots[knotIndex+1].rotation.eulerAngles);
                if (ratio <= 0f && knotIndex == 0)
                {
                    rotation = Knots[0].rotation.eulerAngles;
                }
                //else if (ratio >= 0.99999f && knotIndex == Beziers.SegmentCount - 1)
                //{
                //    rotation = Knots[Knots.Count - 1].rotation.eulerAngles;
                //}
                //else
                {
                    // rotation = Vector3.Lerp(Knots[knotIndex].rotation.eulerAngles, BezierUtil.ClosestAngle(Knots[knotIndex].rotation.eulerAngles, Knots[knotIndex+1].rotation.eulerAngles), ratio);
                    rotation = Vector3.Lerp(Knots[knotIndex].rotation.eulerAngles, Knots[knotIndex + 1].rotation.eulerAngles, ratio);

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
            if (!Beziers.IsCalcArcLengthWithT) Beziers.CalcArcLengthWithT(IsLoop);

            return Beziers.TotalLength / time;
        }


        public void Render()
        {

            var output = Output(step, IsLoop);

                for (int i = 0; i < bezierObject.Count; i++)
                {
                    Destroy(bezierObject[i]);
                }
                bezierObject.Clear();

                for (int i = 1; i < Beziers.SegmentCount - 1; i++)
                {
                    bezierObject.Add(new GameObject("bezierControl" + i));
                    bezierObject[i - 1] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bezierObject[i - 1].transform.position = Beziers[i, 0];

                    bezierObject[i - 1].transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    bezierObject[i - 1].transform.parent = this.transform;
                    bezierObject[i - 1].GetComponent<Renderer>().material.color = Color.red;
                }


                if (render != null)
                {
                    //cube = new GameObject[output.Length];

                    render.material = new Material(Shader.Find("Sprites/Default"));
                    render.positionCount = output.Length;
                    render.startWidth = 0.1f;
                    render.endWidth = 0.1f;
                    render.startColor = Color.white;
                    render.endColor = Color.black;
                    for (int i = 0; i < output.Length; i++)
                    {
                        render.SetPosition(i, output[i]);
                    }
                }


                for (int i = 0; i < inputCube.Count; i++)
                {
                    Destroy(inputCube[i]);
                }
                inputCube.Clear();
                for (int i = 0; i < Knots.Count; i++)
                {
                    inputCube.Add(new GameObject("inputCube" + i));
                    inputCube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    inputCube[i].transform.position = Knots[i].position;
                    inputCube[i].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    inputCube[i].transform.parent = this.transform;

                    inputCube[i].GetComponent<Renderer>().material.color = Color.blue;
                }



                if (inputCube != null && inputCube.Count != 0)
                {
                    for (int i = 0; i < Knots.Count - 1; i++)
                    {
                        inputCube[i].transform.position = Knots[i].position;
                        inputCube[i].transform.rotation = Knots[i].rotation;
                    }
                }
                if (Knots.Count > inputCube.Count)
                {
                    inputCube.RemoveAt(inputCube.Count - 1);
                }

            
            Debug.Log("Rendered");
        }
    }
}