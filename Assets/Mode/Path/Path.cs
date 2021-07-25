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
    public interface ISerialize
    {
    }
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
        public float TotalLength => Beziers.TotalLength;
        public int BeziersPointsLength => Beziers.Points.Length;

        private int Iteration = 5;
        [SerializeField]
        public bool IsLoop= false;
        [SerializeField]
        public bool IsCoordinateControl = false;
        [SerializeField]
        public ushort Time = 10;
        [SerializeField]
        public ushort Step = 5;

        [SerializeField]
        public bool IsCameraShake { get; set; }
        //public PerlinCameraShake CameraShake;

        protected ControlPoint DefaultPosition { get; set; }

        public Render render;
        public Serializer serializer;

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
            render = new Render(this);
            serializer = new Serializer(this);

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

            render.Display();
        }
        private Vector3 _nowMousePosi; // 現在のマウスのワールド座標

        /// <summary>
        /// カーソルの位置のknotを取得する
        /// Get the knot at the cursor position
        /// </summary>
        public int GetCursorPositionKnot(out GameObject selectedKnot)
        {
            var output = Output(Step, IsLoop);

            if (render.LineMesh != null)
            {
                render.LineMesh.RenderPipe(output);
            }
            int i = -1;
            render.bezierObject.Select(g => g.layer = 0);
            Vector3 pos = Input.mousePosition;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out hit))
            {
                selectedKnot = render.bezierObject.Find(g => g == hit.collider.gameObject);
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
            render.bezierObject.Select(g =>g.layer = 0);

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
                render.Display();
            }
            render.bezierObject.Select(g => g.layer = 2);
        }

        /// <summary>
        /// Path上のカーソルの位置を取得する
        /// Get the position of the cursor on the Path
        /// </summary>
        public float GetCursorPositionPath()
        {
            if (render.LineMesh == null)
            {
                var output = Output(Step, IsLoop);
                render.LineMesh.RenderPipe(output);
            }

            render.LineMesh.gameObject.layer = 0;

            float t = 0;
            Vector3 pos = Input.mousePosition;
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out hit))
            {
                t = findClosest(hit.point);
            }

            render.LineMesh.gameObject.layer = 2;
            return t;
        }

        /// <summary>
        /// カメラモーションを再生する
        /// Play camera motion
        /// </summary>
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
                    render.moveCameraCube.transform.position = pos;
                    render.moveCameraCube.transform.rotation = rot;
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
            render.moveCameraCube.transform.position = DefaultPosition.position;
            render.moveCameraCube.transform.rotation = DefaultPosition.rotation;
            yield break;
        }

        private Vector3 CalcPosition(int bezierIndex, float t)
        {
            return BezierUtil.Position(Beziers[bezierIndex, 0], Beziers[bezierIndex, 1], Beziers[bezierIndex, 2], t % 1);
        }

        private Quaternion CalcRotation(int knotIndex, float ratio)
        {
            Quaternion rotation = Squad.Spline(Knots, knotIndex, Knots.Count, ratio);

            if (LookAts.Count != 0)
            {
                //if (Knots[knotIndex].isLookAt || Knots[knotIndex+1].isLookAt)
                //{
                //    rotation = Vector3.Lerp((Knots[knotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation),
                //                           (Knots[knotIndex+1].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation), ratio);
                //}
            }

            rotation.z = 0f;
            rotation.w = 0f;
            return rotation;
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

        /// <summary>
        /// stepごとの座標のリストを返す
        /// Returns a list of coordinates for each step
        /// </summary>
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
                render.moveCameraCube.transform.position = Knots[0].position;
            }
            SetBezierFromKnots();
        }
        public void AddKnot(ControlPoint cp, float? t = null)
        {
            if (t != null)
            {
                var ft = (float)t;
                int bezierIndex = (int)Math.Floor(ft);
                int knotIndex = bezierIndex % 2 == 0 ? (bezierIndex / 2) : (bezierIndex + 1) / 2;
                Debug.Log("ft:" + ft + " " + bezierIndex + " " + knotIndex);
                Vector3 position = CalcPosition((int)Math.Floor(ft), ft % 1);
                Quaternion rotation = Quaternion.Lerp(Knots[knotIndex].rotation, Knots[knotIndex].rotation, bezierIndex % 2 == 0 ? ft % 1 : ft);
                float fov = Mathf.Lerp(Knots[knotIndex].fov, Knots[knotIndex].fov, bezierIndex % 2 == 0 ? ft % 1 : ft);

                this.Knots.Insert(knotIndex + 1, new ControlPoint(position, rotation, fov));
            }
            else
            {
                this.Knots.Add(cp);
                if (Knots.Count == 0)
                {
                    render.moveCameraCube.transform.position = Knots[0].position;
                }
            }

            SetBezierFromKnots();
        }
        public void RemoveKnot()
        {
            this.Knots.RemoveAt(Knots.Count - 1);
            SetBezierFromKnots();
        }
        /// <summary>
        /// 中間地点にknotを追加する
        /// Add a Knot in the middle
        /// </summary>
        public void AddKnotMiddle()
        {
            if (render.LineMesh == null)
            {
                var output = Output(Step, IsLoop);
                render.LineMesh.RenderPipe(output);
            }
            render.LineMesh.gameObject.layer = 0;

            float t = GetCursorPositionPath();
            AddKnot(CameraUtil.CameraPosition(), t);
            Debug.Log("Insert Knot Succeed");
            render.Display();

            render.LineMesh.gameObject.layer = 2;

        }
        public void AddLookAt(Vector3 position, Quaternion rotation, float fov)
        {
            this.LookAts.Add(new ControlPoint(position, rotation, fov));
            SetBezierFromKnots();
        }
        public void AddLookAt(ControlPoint cp, float? t = null)
        {
            //TODO tを指定した場合の処理を追記する
            this.LookAts.Add(cp);
            SetBezierFromKnots();
        }
        public void RemoveLookAt()
        {
            this.LookAts.RemoveAt(Knots.Count - 1);
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
            render.moveCameraCube.transform.position = CalcPosition((int)Math.Floor(t), t % 1);

            return t;
        }

        public class Render
        {
            Path Path;
            public GameObject moveCameraCube;
            public LineRenderer LineRenderer;
            public List<GameObject> inputCube = new List<GameObject>();
            public List<GameObject> bezierObject = new List<GameObject>();

            public PipeMeshGenerator LineMesh;

            public Render(Path Path)
            {
                this.Path = Path;
                LineRenderer = Path.gameObject.AddComponent<LineRenderer>();
                // CameraShake = Path.AddComponent<PerlinCameraShake>();
                // CameraShake.enabled = false;
                moveCameraCube = new GameObject("moveCameraCube");
                moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
                moveCameraCube.transform.parent = Path.gameObject.transform;
                moveCameraCube.layer = 2;
                LineMesh = Path.gameObject.AddComponent<PipeMeshGenerator>();
                LineMesh.gameObject.layer = 2;
            }

            public void Display()
            {

                var output = Path.Output(Path.Step, Path.IsLoop);

                for (int i = 0; i < bezierObject.Count; i++)
                {
                    Destroy(bezierObject[i]);
                }
                bezierObject.Clear();

                for (int i = 1; i < Path.Beziers.SegmentCount - 1; i++)
                {
                    bezierObject.Add(new GameObject("bezierControl" + i));
                    bezierObject[i - 1] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bezierObject[i - 1].transform.position = Path.Beziers[i, 0];

                    bezierObject[i - 1].transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    bezierObject[i - 1].transform.parent = Path.gameObject.transform;
                    bezierObject[i - 1].GetComponent<Renderer>().material.color = Color.red;
                    bezierObject[i - 1].layer = 2;
                }


                if (LineRenderer != null)
                {
                    //cube = new GameObject[output.Length];

                    LineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    LineRenderer.positionCount = output.Length;
                    LineRenderer.startWidth = 0.1f;
                    LineRenderer.endWidth = 0.1f;
                    LineRenderer.startColor = Color.white;
                    LineRenderer.endColor = Color.black;
                    for (int i = 0; i < output.Length; i++)
                    {
                        LineRenderer.SetPosition(i, output[i]);
                    }
                }

                for (int i = 0; i < inputCube.Count; i++)
                {
                    Destroy(inputCube[i]);
                }
                inputCube.Clear();
                for (int i = 0; i < Path.Knots.Count; i++)
                {
                    inputCube.Add(new GameObject("inputCube" + i));
                    inputCube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    inputCube[i].transform.position = Path.Knots[i].position;
                    inputCube[i].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    inputCube[i].transform.parent = Path.gameObject.transform;
                    inputCube[i].layer = 2;

                    inputCube[i].GetComponent<Renderer>().material.color = Color.blue;
                }

                if (inputCube != null && inputCube.Count != 0)
                {
                    for (int i = 0; i < Path.Knots.Count - 1; i++)
                    {
                        inputCube[i].transform.position = Path.Knots[i].position;
                        inputCube[i].transform.rotation = Path.Knots[i].rotation;
                    }
                }
                if (Path.Knots.Count > inputCube.Count)
                {
                    inputCube.RemoveAt(inputCube.Count - 1);
                }

                Debug.Log("Rendered");
            }
        }
        public class Serializer : ISerialize
        {
            Path Path;

            public Serializer(Path Path)
            {
                this.Path = Path;
            }

            public void ToXML()
            {
                var xml = new XDocument(
                     new XElement("Paths",
                          new XElement("Path",
                               new XElement("Knots", Path.Knots),
                               new XElement("LockAts", Path.LookAts),
                               new XElement("Iteration", Path.Iteration),
                               new XElement("IsLoop", Path.IsLoop),
                               new XElement("Time", Path.Time),
                               new XElement("IsCameraShake", Path.IsCameraShake)
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
}