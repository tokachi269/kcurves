using MonitorComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets
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
        
        private int Iteration = 5;
        [SerializeField]
        public bool IsLoop= false;
        [SerializeField]
        public bool IsCoordinateControl = false;
        [SerializeField]
        public ushort Time = 10;
        [SerializeField]
        public ushort Step = 10;

        [SerializeField]
        public bool IsCameraShake { get; set; }
        //public PerlinCameraShake CameraShake;

        protected ControlPoint DefaultPosition { get; set; }

        public GameObject moveCameraCube;

        public static List<GameObject> inputCube = new List<GameObject>();
        public List<GameObject> bezierObject = new List<GameObject>();

        public static PipeMeshGenerator Line;

        Monitor monitor;
        MonitorInput diffTMonitor;
        MonitorInput distMonitor;
        MonitorInput befTMonitor;
        /* Debug用変数 **/
        [SerializeField]
        public float dist = 0;
        [SerializeField]
        public float distall = 0;
        [SerializeField]
        public float diffT = 0;
        [SerializeField]
        private Vector3 bef = Vector3.zero;
        [SerializeField]
        private float befT = 0;
        /* Debug用変数ここまで **/

        void Update()
        {
          //  diffTMonitor.Sample(diffT);
           // distMonitor.Sample(dist);
           // befTMonitor.Sample(befT);
        }

            public void Awake()
        {
            // CameraShake = gameObject.AddComponent<PerlinCameraShake>();
            // CameraShake.enabled = false;
            moveCameraCube = new GameObject("moveCameraCube");
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.parent = this.transform;
            moveCameraCube.layer = 2;
            Line = gameObject.AddComponent<PipeMeshGenerator>();
            Line.gameObject.layer = 2;

            monitor = new Monitor("monitor");
            monitor.Mode = ValueAxisMode.Fixed;
            monitor.Max = 1f;
            diffTMonitor = new MonitorInput(monitor, "diffT", Color.red);
            distMonitor = new MonitorInput(monitor, "dist", Color.magenta);
            befTMonitor = new MonitorInput(monitor, "T", Color.magenta);

        }

        public void CoordinateControl()
        {
            //TODO
            IsCoordinateControl = !IsCoordinateControl;
        }

        void OnValidate()
        {
            SetBezierFromKnots();
        }

        /// <summary>
        /// ControlPointのリストからPathを算出する
        /// Calculating a Path from a list of ControlPoints
        /// </summary>
        public void SetBezierFromKnots()
        {
            Beziers = KCurves.CalcBeziers(Knots.Where(data => data.applyItems.position == true).Select(data => data.position).ToArray(), Iteration, IsLoop) as ExtendBezierControls;
            if (Beziers.SegmentCount >= 3)
            {
                int segment;
                Vector3[] divVec = SplitSegments(out segment);
                Beziers = new ExtendBezierControls(segment, divVec, IsLoop);
            }

            Render();
        }
        private Vector3 _nowMousePosi; // 現在のマウスのワールド座標

        /// <summary>
        /// カーソルの位置のknotを取得する
        /// Get the knot at the cursor position
        /// </summary>
        public int GetCursorPositionKnot(out GameObject selectedKnot)
        {
            int i = -1;
            bezierObject.Select(g => g.layer = 0);
            Vector3 pos = Input.mousePosition;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out hit))
            {
                selectedKnot = bezierObject.Find(g => g == hit.collider.gameObject);
            }
            else
            {
                selectedKnot = null;
            }

            return i;
        }

        /// <summary>
        /// knotを移動させる
        /// Move the knot
        /// </summary>
        public void MoveKnot()
        {
            bezierObject.Select(g =>g.layer = 0);

            Vector3 nowmouseposi = Input.mousePosition;
            Vector3 diffposi;
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(nowmouseposi);

            GetCursorPositionKnot(out GameObject selectedKnot);

            if (Physics.Raycast(ray, out hit))
            {
                // 現在のマウスのワールド座標を取得
                nowmouseposi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // 一つ前のマウス座標との差分を計算して変化量を取得
                diffposi = nowmouseposi - _nowMousePosi;
                //　Y成分のみ変化させる
                diffposi.x = 0;
                diffposi.z = 0;
                // 開始時のオブジェクトの座標にマウスの変化量を足して新しい座標を設定
                GetComponent<Transform>().position += diffposi;
                // 現在のマウスのワールド座標を更新
                _nowMousePosi = nowmouseposi;

                Debug.Log("Move Knot Succeed");
                Render();
            }
            bezierObject.Select(g => g.layer = 2);

        }

        /// <summary>
        /// Path上のカーソルの位置を取得する
        /// Get the position of the cursor on the Path
        /// </summary>
        public float GetCursorPositionPath()
        {
            Line.gameObject.layer = 0;

            float t = 0;
            Vector3 pos = Input.mousePosition;
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out hit))
            {
                t = findClosest(hit.point);
            }

            Line.gameObject.layer = 2;
            return t;
        }

        /// <summary>
        /// 中間地点にknotを追加する
        /// Add a Knot in the middle
        /// </summary>
        public void AddKnotMiddle()
        {
            Line.gameObject.layer = 0;

            float t = GetCursorPositionPath();
            AddKnot(CameraUtil.CameraPosition(), t);
            Debug.Log("Insert Knot Succeed");
            Render();

            Line.gameObject.layer = 2;

        }

        public IEnumerator Play()
        {
            DefaultPosition = CameraUtil.CameraPosition();

            if (IsCameraShake)
            {
           //     CameraShake.enabled = true;
            }

            float maxSpeed = MaxSpeed(Time);
            int knotIndex = 0;
            int bezierIndex = 0;

            float progressLength = 0f;
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
                    if (progressLength >= Beziers.Length(bezierIndex))
                    {
                        progressLength -= Beziers.Length(bezierIndex);
                        isSegChanged = true;
                    }
                }
                else if (bezierIndex % 2 == 1)
                {
                    if (progressLength >= Beziers.Length(bezierIndex))
                    {
                        isSegChanged = true;
                    }
                }
                else
                {
                    if (progressLength >= Beziers.Length(bezierIndex - 1) + Beziers.Length(bezierIndex))
                    {
                        progressLength -= Beziers.Length(bezierIndex - 1) + Beziers.Length(bezierIndex);
                        isSegChanged = true;
                    }
                }


                if (isSegChanged)
                {
                    bezierIndex++;

                    knotIndex = bezierIndex % 2 == 0 ? (bezierIndex / 2) : (bezierIndex + 1) / 2;
                    //mode = EasingMode.None;
                    mode = (EasingMode)((byte)easingMode[knotIndex + 1] | ((byte)easingMode[knotIndex] << 1));
                    KnotBetweenRange = 0f;
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
                        break;
                    }
                }

                // Debug.Log("mode:" + mode + " bezierIndex:" + bezierIndex + " knotIndex:" + knotIndex + " KnotBetweenRange:" + KnotBetweenRange+ "ProgressLength" + progressLength);

                float easing = Easing.GetEasing(mode, progressLength / KnotBetweenRange);

                //Debug.Log("easing:"+ easing);

                float t = Beziers.GetT(bezierIndex, bezierIndex != 0 && bezierIndex % 2 == 0 ? easing * KnotBetweenRange - Beziers.Length(bezierIndex-1) : easing*KnotBetweenRange);

                //Debug.Log("t:"+ t);

                //Debug.Log("bezierIndex:" + bezierIndex + "  ProgressLength:" + ProgressLength + "maxS:" + maxSpeed + "currentTime:" + currentTime);
                {
                    diffT = t - befT;
                    //Debug.Log("t:" + diffT);
                    befT = t;
                    Vector3 now = CalcPosition(bezierIndex, t);
                
                    distall += dist;
                    dist = Vector3.Distance(bef, now);
                    bef = now;
                    //Debug.Log("dist:" + dist);
                }

                if (t <= Beziers.SegmentCount)
                {
                    Vector3 pos = CalcPosition(bezierIndex, t);
                    Quaternion rot = CalcRotation(knotIndex, easing);
                    moveCameraCube.transform.position = pos;
                    moveCameraCube.transform.rotation = rot;
                    GameObject.Find("Main Camera").transform.position = pos;
                    GameObject.Find("Main Camera").transform.rotation = rot;
                }

                float dt = UnityEngine.Time.deltaTime;

                progressLength += maxSpeed * dt;
                currentTime += dt;
                yield return null;
            }
            if (IsCameraShake)
            {
           //     CameraShake.enabled = false;
            }
            moveCameraCube.transform.position = DefaultPosition.position;
            moveCameraCube.transform.rotation = DefaultPosition.rotation;
            yield break;
        }

        private Vector3 CalcPosition(int bezierIndex, float t)
        {
            return BezierUtil.Position(Beziers[bezierIndex, 0], Beziers[bezierIndex, 1], Beziers[bezierIndex, 2], t % 1);
        }

        private Quaternion CalcRotation(int knotIndex, float ratio)
        {

            Quaternion rotation = Squad.Spline(Knots, knotindex, Knots.Count, ratio);
           // Quaternion rotation = GetPoint(knotIndex,ratio);
            // float easing = Easing.EaseInOutSine(ratio);




            if (LookAts.Count != 0)
            {
                //if (Knots[knotIndex].isLookAt || Knots[knotIndex+1].isLookAt)
                //{
                //    rotation = Vector3.Lerp((Knots[knotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation),
                //                           (Knots[knotIndex+1].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation), ratio);
                //}
            }

            rotation.z = 0f;
            return rotation;
        }

        public Quaternion GetPoint(int knotIndex, float t)
        {

            Vector3 eulerAngles = Knots[knotIndex].rotation.eulerAngles;
            Vector3 a = ClosestAngle(eulerAngles, Knots[knotIndex+1].rotation.eulerAngles);
            Vector3 a2;
            if (knotIndex > 0)
            {
                a2 = 0.5f * (a - ClosestAngle(eulerAngles, Knots[knotIndex-1].rotation.eulerAngles));
            }
            else
            {
                a2 = a - eulerAngles;
            }
            Vector3 a3;
            if (knotIndex < Knots.Count - 2)
            {
                a3 = 0.5f * (ClosestAngle(a, Knots[knotIndex+2].rotation.eulerAngles) - eulerAngles);
            }
            else
            {
                a3 = a - eulerAngles;
            }

            return Quaternion.Euler((2f * t * t * t - 3f * t * t + 1f) * eulerAngles + (t * t * t - 2f * t * t + t) * a2 + (-2f * t * t * t + 3f * t * t) * a + (t * t * t - t * t) * a3);

            //return Quaternion.Euler(GetCatmullRomPosition(p0, p1, p2, p3, ratio));
        }

        Vector3 GetCatmullRomPosition(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float ratio)
        {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * ratio) + (c * ratio * ratio) + (d * ratio * ratio * ratio));

            return pos;
        }

        public static Vector3 ClosestAngle(Vector3 a, Vector3 b)
        {
           //Vector3 vector = a - b;
           //if (vector.x > 180f)
           //{
           //    b.x += 360f;
           //}
           //else if (vector.x < -180f)
           //{
           //    b.x -= 360f;
           //}
           //if (vector.y > 180f)
           //{
           //    b.y += 360f;
           //}
           //else if (vector.y < -180f)
           //{
           //    b.y -= 360f;
           //}
           //if (vector.z > 180f)
           //{
           //    b.z += 360f;
           //}
           //else if (vector.z < -180f)
           //{
           //    b.z -= 360f;
           //}
            return b;
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

        /// <summary>
        /// セグメントを分割してユーザー制御点をベジェ制御点に追加する
        /// Split segments and add user control points to Bezier control points
        /// </summary>
        // ユーザー制御点をベジェ制御点に追加する
        private Vector3[] SplitSegments(out int dividedSegmentCount)
        {
           // Debug.Log("Beziers.SegmentCount:" + Beziers.SegmentCount);

            if (IsLoop)
            {
                dividedSegmentCount = (Beziers.SegmentCount * 2 - 1);
            }
            else
            { 
                dividedSegmentCount = (Beziers.SegmentCount - 2) * 2;
            }
            
           // Debug.Log("dividedSegmentCount:" + dividedSegmentCount);
            Vector3[] dividedPoints = new Vector3[IsLoop ? dividedSegmentCount * 2 + 4 : dividedSegmentCount * 2 + 1];

            ushort segCnt = (ushort)(IsLoop ? Beziers.SegmentCount : Beziers.SegmentCount - 1);
           // Debug.Log("segCnt" + segCnt);

            //var tss = Beziers.Ts.Select((num, index) => (num, index));
            //foreach (var t in tss) Debug.Log(t.index+"," + t.num);

            ushort index = 0;
            int i = IsLoop ? 0 : 1;
            for (; i < segCnt; i++)
            {
               // Debug.Log("i=" + i +","+ Beziers[i, 0] +","+ Beziers[i, 1] + "," + Beziers[i, 2] + "," + (float)Beziers.Ts[i]);
                Vector3[] result = BezierUtil.Divide(Beziers[i, 0], Beziers[i, 1], Beziers[i, 2], (float)Beziers.Ts[i]);
                int condition = (i == segCnt - 1 && IsLoop) ? 4 : 3;
                for (int j=0; j <= condition; j++,index++)
                {
                  //  Debug.Log("dividedPoints" + dividedPoints.Length+" index:"+index + ", "+result[j]);
                    dividedPoints[index] = result[j];
                }

            }
            if (!IsLoop) dividedPoints[dividedPoints.Length - 1] = Beziers[segCnt,0];
            //Debug.Log(dividedPoints.Length - 1 + ", " + dividedPoints[dividedPoints.Length - 1]);
            return dividedPoints;
        }

        public Vector3[] Output(ushort step, bool isLoop)
        {
            if (Beziers is null)
            {
                SetBezierFromKnots();
            }

            return Beziers.CalcPlots(step, isLoop);
        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov)
        {
            this.Knots.Add(new ControlPoint(position, rotation, fov));
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
            this.LookAts.Add(new ControlPoint(position, rotation, fov));
            SetBezierFromKnots();
        }

        public void RemoveLookAt()
        {
            this.LookAts.RemoveAt(Knots.Count - 1);
            SetBezierFromKnots();
        }

        public void AddKnot(ControlPoint cp, float? t = null)
        {
            if(t!= null)
            {
                var ft = (float)t;
                int bezierIndex = (int)Math.Floor(ft);
                int knotIndex = bezierIndex % 2 == 0 ? (bezierIndex / 2) : (bezierIndex + 1) / 2;
                Debug.Log("ft:"+ ft + " " + bezierIndex + " " + knotIndex);
                Vector3 position = CalcPosition((int)Math.Floor(ft), ft % 1);
                Quaternion rotation = Quaternion.Lerp(Knots[knotIndex].rotation, Knots[knotIndex].rotation, bezierIndex % 2 == 0 ? ft % 1 : ft) ;
                float fov= Mathf.Lerp(Knots[knotIndex].fov, Knots[knotIndex].fov, bezierIndex % 2 == 0 ? ft % 1 : ft);

                this.Knots.Insert(knotIndex+1, new ControlPoint(position, rotation, fov));
            }
            else
            {
                this.Knots.Add(cp);
                if (Knots.Count == 0)
                {
                    moveCameraCube.transform.position = Knots[0].position;
                }
            }

            SetBezierFromKnots();
        }


        public void AddLookAt(ControlPoint cp)
        {
            this.LookAts.Add(cp);
            SetBezierFromKnots();
        }

        private float MaxSpeed(int time)
        {
            if (!Beziers.IsCalcArcLengthWithT) Beziers.CalcArcLengthWithT(IsLoop);

            return Beziers.TotalLength / time;
        }

        public float findClosest(Vector3 cursor)
        {
            int step = 5;
            int index = 5;
            var output = Output(5, IsLoop);
            float distance = float.MaxValue;
            foreach (var v in output.Select((p, i) => new { p, i }))
            {
                dist = Vector3.Distance(v.p, cursor);
                if (dist < distance)
                {
                    distance = dist;
                    index = v.i;
                }
            }
            float t = (float)index / step;
            distance = float.MaxValue;
            for ( float j = 1; j > 0.01; j /= 2 )
            {
                if (distance < 0.01) break;
                for (float k= t - j; k <= t + j; k += j/2)
                {
                    if (0> k || Beziers.SegmentCount < k) continue;
                    dist = Vector3.Distance(CalcPosition((int)Math.Floor(k), k % 1), cursor);
                    if (dist < distance)
                    {
                        distance = dist;
                        t = k;
                    }
                }
            }
            Debug.Log(t);
            moveCameraCube.transform.position = CalcPosition((int)Math.Floor(t), t % 1);

            return t;
        }


        public void Render()
        {

            var output = Output(Step, IsLoop);

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
                    bezierObject[i - 1].layer = 2;
            }
            

            if (Line != null)
            {
                Line.points = output.ToList();
                Line.RenderPipe();
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
                    inputCube[i].layer = 2;

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
        public void ToXML()
        {
            var xml = new XDocument(
                 new XElement("Paths" ,
                      new XElement("Path",
                           new XElement("Knots", Knots),
                           new XElement("LockAts", LookAts),
                           new XElement("Iteration", Iteration),
                           new XElement("IsLoop", IsLoop),
                           new XElement("Time", Time),
                           new XElement("IsCameraShake", IsCameraShake)
                      )
                 )
            );
            xml.Save(@"C:\Temp\LINQ_to_XML_Sample2.xml");
        }

        public void Serialize(string filename)
        {
            string text = System.IO.Path.Combine(CameraDirector.RecoveryDirectory, filename + ".xml");
            try
            {
                if (!Directory.Exists(CameraDirector.RecoveryDirectory))
                {
                    Directory.CreateDirectory(CameraDirector.RecoveryDirectory);
                }

                using (FileStream fileStream = new FileStream(text, FileMode.OpenOrCreate))
                {
                    fileStream.SetLength(0L);
                    new XmlSerializer(this.GetType()).Serialize(fileStream, this);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        //public void Deserialize(string filename)
        //{
        //    string path = System.IO.Path.Combine(CameraDirector.RecoveryDirectory, filename + ".xml");
        //    if (File.Exists(path))
        //    {
        //        List<ControlPoint> list = new List<ControlPoint>();
        //        try
        //        {
        //            using (FileStream fileStream = new FileStream(path, FileMode.Open))
        //            {
        //                list = (new XmlSerializer(list.GetType()).Deserialize(fileStream) as List<ControlPoint>);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogException(e);
        //        }
        //        if (list != null)
        //        {
        //            this.Knots.Clear();
        //            foreach (ControlPoint item in list)
        //            {
        //                this.Knots.Add(item);
        //            }
        //        }
        //    }
        //}
    }
}