using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace CameraOperator.Tool
{
    public interface ISerialize
    {
        public void ToXML();
        public void Serialize(string filename);
        public void Deserialize(string filename);
    }

    public class Path : BaseCameraMode, ICameraMode
    {
        public override string Name { get; set; }

        //���[�U�[����_
        protected override List<CameraConfig> Knots { get; set; } = new List<CameraConfig>();
        public int KnotsCount => Knots.Count;

        //���[�U�[���䒍���_
        protected List<CameraConfig> LookAts { get; private set; } = new List<CameraConfig>();
        public int LookAtsCount => Knots.Count;

        //Bezier�v�Z����
        protected ExtendBezierControls Positions { get; private set; }
        public int BeziersCount => Positions.SegmentCount;
        public float TotalLength => Positions.TotalLength;
        public int BeziersPointsLength => Positions.Points.Length;

        protected ExtendBezierControls Rotations { get; private set; }
        public int RotationsCount => Positions.SegmentCount;
        public int RotationsPointsLength => Positions.Points.Length;

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

        protected CameraConfig defaultCameraConfig { get; set; }

        public Render render;
        public Serializer serializer;
        public PerlinCameraShake CameraShake;

        /* Debug�p�ϐ� **/
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
        /* Debug�p�ϐ������܂� **/

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
            CameraShake = new PerlinCameraShake();
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
        /// ControlPoint�̃��X�g����Path���Z�o����
        /// Calculating a Instance from a list of ControlPoints
        /// </summary>
        public void SetBezierFromKnots()
        {
            Positions = KCurves.CalcBeziers(Knots.Where(data => data.applyItems.position == true).Select(data => data.position).ToArray(), Iteration, IsLoop) as ExtendBezierControls;
            if (Positions.SegmentCount >= 3)
            {
                int segment;
                Vector3[] divPositions = SplitSegments(out segment);
                Positions = new ExtendBezierControls(segment, divPositions, IsLoop);
            }

            Rotations = KCurves.CalcBeziers(Knots.Where(data => data.applyItems.position == true).Select(data => data.rotation.eulerAngles).ToArray(), Iteration, IsLoop) as ExtendBezierControls;
            
            if (Rotations.SegmentCount >= 3)
            {
                int segment;
                Vector3[] divRotations = SplitRotations(out segment);
                Rotations = new ExtendBezierControls(Rotations.SegmentCount, divRotations, IsLoop);
            }

            render.Display();
        }
        private Vector3 _nowMousePosi; // ���݂̃}�E�X�̃��[���h���W

        /// <summary>
        /// �J�[�\���̈ʒu��knot���擾����
        /// Get the knot at the cursor position
        /// </summary>
        public int GetCursorPositionKnot(out GameObject selectedKnot)
        {
            var output = Output(Step, IsLoop);

            var outputRot = OutputRotations(Step, IsLoop);

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
        /// knot���ړ�������
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
                // ���݂̃}�E�X�̃��[���h���W���擾
                nowmouseposi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // ��O�̃}�E�X���W�Ƃ̍������v�Z���ĕω��ʂ��擾
                diffposi = nowmouseposi - _nowMousePosi;
                //�@Y�����̂ݕω�������
                diffposi.x = 0;
                diffposi.z = 0;
                // �J�n���̃I�u�W�F�N�g�̍��W�Ƀ}�E�X�̕ω��ʂ𑫂��ĐV�������W��ݒ�
                GetComponent<Transform>().position += diffposi;
                // ���݂̃}�E�X�̃��[���h���W���X�V
                _nowMousePosi = nowmouseposi;

                Debug.Log("Move Knot Succeed");
                render.Display();
            }
            render.bezierObject.Select(g => g.layer = 2);
        }

        /// <summary>
        /// Path��̃J�[�\���̈ʒu���擾����
        /// Get the position of the cursor on the Instance
        /// </summary>
        public float GetCursorPositionPath()
        {
            if (render.LineMesh == null)
            {
                var output = Output(Step, IsLoop);
                var outputRot = OutputRotations(Step, IsLoop);
                Debug.Log(outputRot);
                List<Vector3> mergedList = new List<Vector3>(output.Length + outputRot.Length);
                mergedList.AddRange(output);
                mergedList.AddRange(outputRot);

                render.LineMesh.RenderPipe(mergedList.ToArray());
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
        /// �J�������[�V�������Đ�����
        /// Play camera motion
        /// </summary>
        public IEnumerator Play()
        {
            List<CameraConfig> tempKnots = Knots;
            defaultCameraConfig = CameraUtil.CameraPosition();

            if (IsCameraShake)
            {
               // CameraShake.enabled = true;
            }

            float maxSpeed = MaxSpeed(Time);
            int knotIndex = 0;
            int bezierIndex = 0;

            float progressLength = 0f;
            var easingMode = SetEasingMode();

            float KnotBetweenRange = Positions.Length(0);

            // ��~���Ԃ��ݒ肳��Ă���ꍇ�A�w��b���ԏ�����ҋ@
            if (tempKnots[0].delay != 0f)
            {
                yield return new WaitForSeconds(tempKnots[0].delay);
            }

            EasingMode mode = (EasingMode)((byte)easingMode[1] | ((byte)easingMode[0] << 1));

            // 
            for (float currentTime= 0; ;)
            {
                //���̃Z�O�����g�Ɉړ����������肷��
                bool isSegChanged = false;
                if (bezierIndex == 0 || bezierIndex == Positions.SegmentCount)
                {
                    if (progressLength >= Positions.Length(bezierIndex))
                    {
                        progressLength -= Positions.Length(bezierIndex);
                        isSegChanged = true;
                    }
                }
                else if (bezierIndex % 2 == 1)
                {
                    if (progressLength >= Positions.Length(bezierIndex))
                    {
                        isSegChanged = true;
                    }
                }
                else
                {
                    if (progressLength >= Positions.Length(bezierIndex - 1) + Positions.Length(bezierIndex))
                    {
                        progressLength -= Positions.Length(bezierIndex - 1) + Positions.Length(bezierIndex);
                        isSegChanged = true;
                    }
                }

                //�Z�O�����g���ړ������ꍇ�A�v�Z�ɕK�v�ȃp�����[�^�[��ݒ肷��
                if (isSegChanged)
                {
                    bezierIndex++;

                    knotIndex = bezierIndex % 2 == 0 ? (bezierIndex / 2) : (bezierIndex + 1) / 2;
                    //mode = EasingMode.None;
                    mode = (EasingMode)((byte)easingMode[knotIndex + 1] | ((byte)easingMode[knotIndex] << 1));
                    KnotBetweenRange = 0f;

                    if (knotIndex == tempKnots.Count - 1)
                    {
                        KnotBetweenRange = Positions.Length(bezierIndex);
                    }
                    else
                    {
                        for (ushort j = (ushort)(2 * knotIndex - 1); j < Positions.SegmentCount && j <= 2 * knotIndex; j++)
                        {
                            KnotBetweenRange += Positions.Length(j);
                        }
                    }

                    // ��~���Ԃ��ݒ肳��Ă���ꍇ�A�w��b���ԏ�����ҋ@
                    if (tempKnots[knotIndex].delay != 0f)
                    {
                        yield return new WaitForSeconds(tempKnots[knotIndex].delay);
                    }

                    Debug.Log("knotIndex++");
                    if (bezierIndex == Positions.SegmentCount)
                    {
                        break;
                    }
                }

                // Debug.Log("mode:" + mode + " bezierIndex:" + bezierIndex + " knotIndex:" + knotIndex + " KnotBetweenRange:" + KnotBetweenRange+ " ProgressLength" + progressLength);

                float easing = Easing.GetEasing(mode, progressLength / KnotBetweenRange);

                //Debug.Log("easing:"+ easing);

                float t = Positions.GetT(bezierIndex, bezierIndex != 0 && bezierIndex % 2 == 0 ? easing * KnotBetweenRange - Positions.Length(bezierIndex-1) : easing*KnotBetweenRange);

                //Debug.Log("bezierIndex:" + bezierIndex + "  ProgressLength:" + ProgressLength + "maxS:" + maxSpeed + "currentTime:" + currentTime);
                {
                    //debug�p�ϐ�
                    diffT = t - befT;
                    //Debug.Log("param:" + diffT);
                    befT = t;
                    Vector3 now = CalcPosition(bezierIndex, t);
                
                    distall += dist;
                    dist = Vector3.Distance(bef, now);
                    bef = now;
                    //Debug.Log("dist:" + dist);
                }

                // positon��rotation��K�p
                if (t <= Positions.SegmentCount)
                {
                    Vector3 pos = CalcPosition(bezierIndex, t);
                    Quaternion rot = Quaternion.Inverse(Quaternion.Euler(CalcRot(bezierIndex, t)));

                    //Quaternion rot = CalcRotation(ref tempKnots, knotIndex, progressLength / KnotBetweenRange);
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
               // CameraShake.enabled = false;
            }

            GameObject.Find("Main Camera").transform.position = defaultCameraConfig.position;
            GameObject.Find("Main Camera").transform.rotation = defaultCameraConfig.rotation;
            render.moveCameraCube.transform.position = defaultCameraConfig.position;
            render.moveCameraCube.transform.rotation = defaultCameraConfig.rotation;
            yield break;
        }

        private Vector3 CalcPosition(int bezierIndex, float t)
        {
            return BezierUtil.Position(Positions[bezierIndex, 0], Positions[bezierIndex, 1], Positions[bezierIndex, 2], t % 1);
        }
        private Vector3 CalcRot(int bezierIndex, float t)
        {
            return BezierUtil.Position(Rotations[bezierIndex, 0], Rotations[bezierIndex, 1], Rotations[bezierIndex, 2], t % 1);
        }

        private Quaternion CalcRotation(ref List<CameraConfig> Knots,int knotIndex, float ratio)
        {
            Quaternion rotation = Squad.Spline(ref Knots, knotIndex, Knots.Count, ratio);

            if (LookAts.Count != 0)
            {
                if (Knots[knotIndex].isLookAt || Knots[knotIndex+1].isLookAt)
                {
                   // rotation = Vector3.Lerp(Knots[knotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation,
                   //                        Knots[knotIndex+1].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation, ratio);
                }
            }

            // rotation.z = 0f;
            // rotation.w = 0f;
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
        /// ���W�̃Z�O�����g�𕪊����ă��[�U�[����_���x�W�F����_�ɒǉ�����
        /// Split segments and add user control points to Bezier control points
        /// </summary>
        private Vector3[] SplitSegments(out int dividedSegmentCount)
        {
           // Debug.Log("Beziers.SegmentCount:" + Beziers.SegmentCount);

            if (IsLoop)
            {
                dividedSegmentCount = (Positions.SegmentCount * 2 - 1);
            }
            else
            { 
                dividedSegmentCount = (Positions.SegmentCount - 2) * 2;
            }
            
           // Debug.Log("dividedSegmentCount:" + dividedSegmentCount);
            Vector3[] dividedPoints = new Vector3[IsLoop ? dividedSegmentCount * 2 + 4 : dividedSegmentCount * 2 + 1];

            ushort segCnt = (ushort)(IsLoop ? Positions.SegmentCount : Positions.SegmentCount - 1);
           // Debug.Log("segCnt" + segCnt);

            //var tss = Beziers.Ts.Select((num, index) => (num, index));
            //foreach (var param in tss) Debug.Log(param.index+"," + param.num);

            ushort index = 0;
            int i = IsLoop ? 0 : 1;
            for (; i < segCnt; i++)
            {
               // Debug.Log("i=" + i +","+ Beziers[i, 0] +","+ Beziers[i, 1] + "," + Beziers[i, 2] + "," + (float)Beziers.Ts[i]);
                Vector3[] result = BezierUtil.Divide(Positions[i, 0], Positions[i, 1], Positions[i, 2], (float)Positions.Ts[i]);
                int condition = (i == segCnt - 1 && IsLoop) ? 4 : 3;
                for (int j=0; j <= condition; j++,index++)
                {
                  //  Debug.Log("dividedPoints" + dividedPoints.Length+" index:"+index + ", "+result[j]);
                    dividedPoints[index] = result[j];
                }

            }
            if (!IsLoop) dividedPoints[dividedPoints.Length - 1] = Positions[segCnt,0];
            //Debug.Log(dividedPoints.Length - 1 + ", " + dividedPoints[dividedPoints.Length - 1]);
            return dividedPoints;
        }


        /// <summary>
        /// ��]�̃Z�O�����g�𕪊����ă��[�U�[����_���x�W�F����_�ɒǉ�����
        /// Split segments and add user control points to Bezier control points
        /// </summary>
        private Vector3[] SplitRotations(out int dividedSegmentCount)
        {
            // Debug.Log("Beziers.SegmentCount:" + Beziers.SegmentCount);

            if (IsLoop)
            {
                dividedSegmentCount = (Rotations.SegmentCount * 2 - 1);
            }
            else
            {
                dividedSegmentCount = (Rotations.SegmentCount - 2) * 2;
            }

            // Debug.Log("dividedSegmentCount:" + dividedSegmentCount);
            Vector3[] dividedPoints = new Vector3[IsLoop ? dividedSegmentCount * 2 + 4 : dividedSegmentCount * 2 + 1];

            ushort segCnt = (ushort)(IsLoop ? Rotations.SegmentCount : Rotations.SegmentCount - 1);
            // Debug.Log("segCnt" + segCnt);

            //var tss = Beziers.Ts.Select((num, index) => (num, index));
            //foreach (var param in tss) Debug.Log(param.index+"," + param.num);

            ushort index = 0;
            int i = IsLoop ? 0 : 1;
            for (; i < segCnt; i++)
            {
                // Debug.Log("i=" + i +","+ Beziers[i, 0] +","+ Beziers[i, 1] + "," + Beziers[i, 2] + "," + (float)Beziers.Ts[i]);
                Vector3[] result = BezierUtil.Divide(Rotations[i, 0], Rotations[i, 1], Rotations[i, 2], (float)Rotations.Ts[i]);
                int condition = (i == segCnt - 1 && IsLoop) ? 4 : 3;
                for (int j = 0; j <= condition; j++, index++)
                {
                    //  Debug.Log("dividedPoints" + dividedPoints.Length+" index:"+index + ", "+result[j]);
                    dividedPoints[index] = result[j];
                }

            }
            if (!IsLoop) dividedPoints[dividedPoints.Length - 1] = Rotations[segCnt, 0];
            //Debug.Log(dividedPoints.Length - 1 + ", " + dividedPoints[dividedPoints.Length - 1]);
            return dividedPoints;
        }

        /// <summary>
        /// step���Ƃ̍��W�̃��X�g��Ԃ�
        /// Returns a list of coordinates for each step
        /// </summary>
        public Vector3[] Output(ushort step, bool isLoop)
        {
            if (Positions is null)
            {
                SetBezierFromKnots();
            }

            return Positions.CalcPlots(step, isLoop);
        }

        public Vector3[] OutputRotations(ushort step, bool isLoop)
        {
            if (Rotations is null)
            {
                SetBezierFromKnots();
            }
            return Rotations.CalcPlots(step, isLoop);

        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov)
        {
            this.Knots.Add(new CameraConfig(position, rotation, fov));
            if (Knots.Count == 0)
            {
                render.moveCameraCube.transform.position = Knots[0].position;
            }
            SetBezierFromKnots();
        }

        /// <summary>
        /// Knot��ǉ�����
        /// Returns a list of coordinates for each step
        /// </summary>
        /// <param name="cp">�J�����ݒ�</param>
        /// <param name="param">t</param>
        public void AddKnot(CameraConfig cp, float? param = null)
        {
            if (param != null)
            {
                var ft = (float)param;
                int bezierIndex = (int)Math.Floor(ft);
                int knotIndex = bezierIndex % 2 == 0 ? (bezierIndex / 2) : (bezierIndex + 1) / 2;
                Debug.Log("ft:" + ft + " " + bezierIndex + " " + knotIndex);
                Vector3 position = CalcPosition((int)Math.Floor(ft), ft % 1);
                Quaternion rotation = Quaternion.Lerp(Knots[knotIndex].rotation, Knots[knotIndex].rotation, bezierIndex % 2 == 0 ? ft % 1 : ft);
                float fov = Mathf.Lerp(Knots[knotIndex].fov, Knots[knotIndex].fov, bezierIndex % 2 == 0 ? ft % 1 : ft);

                this.Knots.Insert(knotIndex + 1, new CameraConfig(position, rotation, fov));
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
        /// ���Ԓn�_��knot��ǉ�����
        /// Add a Knot in the middle
        /// </summary>
        public void AddKnotMiddle()
        {
            if (render.LineMesh == null)
            {
                var output = Output(Step, IsLoop);
                var outputRot = OutputRotations(Step, IsLoop);
                Debug.Log(outputRot);
                List<Vector3> mergedList = new List<Vector3>(output.Length + outputRot.Length);
                mergedList.AddRange(output);
                mergedList.AddRange(outputRot);

                render.LineMesh.RenderPipe(mergedList.ToArray());
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
            this.LookAts.Add(new CameraConfig(position, rotation, fov));
            SetBezierFromKnots();
        }
        public void AddLookAt(CameraConfig cp, float? t = null)
        {
            //TODO t���w�肵���ꍇ�̏�����ǋL����
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
            if (!Positions.IsCalcTotalLength) {
                Positions.CalcTotalLength(IsLoop);
            }

            return Positions.TotalLength / time;
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
                    if (0> k || Positions.SegmentCount < k) continue;
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
            Path Instance;
            public GameObject moveCameraCube;

            public LineRenderer LineRenderer;
            public LineRenderer LineRendererRotation;  //LineRendererRotation��LineRenderer�P�ڂ�LineRenderer�����\������Ȃ�

            public List<GameObject> inputCube = new List<GameObject>();
            public List<GameObject> inputCubeVector = new List<GameObject>();

            public List<GameObject> bezierObject = new List<GameObject>();

            public PipeMeshGenerator LineMesh;

            public Render(Path instance)
            {
                this.Instance = instance;
                LineRenderer = instance.gameObject.AddComponent<LineRenderer>();
                //�R�����g
                LineRendererRotation = instance.gameObject.AddComponent<LineRenderer>();

                // CameraShake = Instance.AddComponent<PerlinCameraShake>();
                // CameraShake.enabled = false;
                moveCameraCube = new GameObject("moveCameraCube");
                moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                moveCameraCube.transform.localScale = new Vector3(0.2f, 0.2f, 0.5f);
                moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
                moveCameraCube.transform.parent = instance.gameObject.transform;
                moveCameraCube.layer = 2;
                LineMesh = instance.gameObject.AddComponent<PipeMeshGenerator>();
                LineMesh.gameObject.layer = 2;
            }

            public void Display()
            {

                var output = Instance.Output(Instance.Step, Instance.IsLoop);

                var outputRot = Instance.OutputRotations(Instance.Step, Instance.IsLoop);
                Debug.Log("outputRot.Length" + outputRot.Length);
                for (int i = 0; i < bezierObject.Count; i++)
                {
                    Destroy(bezierObject[i]);
                }
                bezierObject.Clear();

                for (int i = 1; i < Instance.Positions.SegmentCount - 1; i++)
                {
                    bezierObject.Add(new GameObject("bezierControl" + i));
                    bezierObject[i - 1] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bezierObject[i - 1].transform.position = Instance.Positions[i, 0];

                    bezierObject[i - 1].transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    bezierObject[i - 1].transform.parent = Instance.gameObject.transform;
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
                if (LineRendererRotation != null)
                {
                    //cube = new GameObject[output.Length];

                    LineRendererRotation.material = new Material(Shader.Find("Sprites/Default"));
                    LineRendererRotation.positionCount = outputRot.Length;
                    LineRendererRotation.startWidth = 0.1f;
                    LineRendererRotation.endWidth = 0.1f;
                    LineRendererRotation.startColor = Color.white;
                    LineRendererRotation.endColor = Color.black;
                    for (int i = 0; i < outputRot.Length; i++)
                    {
                        //Debug.Log(outputRot[i]);
                        LineRendererRotation.SetPosition(i, new Vector3(Quaternion.Euler(outputRot[i].x, outputRot[i].y, outputRot[i].z).x, Quaternion.Euler(outputRot[i].x, outputRot[i].y, outputRot[i].z).y, Quaternion.Euler(outputRot[i].x, outputRot[i].y, outputRot[i].z).z));
                    }
                }

                for (int i = 0; i < inputCube.Count; i++)
                {
                    Destroy(inputCube[i]);
                }

                for (int i = 0; i < inputCubeVector.Count; i++)
                {
                    Destroy(inputCubeVector[i]);
                }

                inputCube.Clear();
                inputCubeVector.Clear();

                for (int i = 0; i < Instance.Knots.Count; i++)
                {
                    inputCube.Add(new GameObject("inputCube" + i));

                    inputCube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    inputCube[i].transform.position = Instance.Knots[i].position;
                    inputCube[i].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    inputCube[i].transform.parent = Instance.gameObject.transform;
                    inputCube[i].layer = 2;
                    inputCube[i].GetComponent<Renderer>().material.color = Color.blue;

                    inputCube.Add(new GameObject("inputCubeVector" + i));


                    inputCubeVector.Add(new GameObject("inputCube" + i));

                    inputCubeVector[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    inputCubeVector[i].transform.position = Instance.Knots[i].position;
                    inputCubeVector[i].transform.localPosition = new Vector3(0, 0, 1f);
                    inputCubeVector[i].transform.localScale = new Vector3(0.05f,  0.05f, 1f);
                    inputCubeVector[i].transform.parent = Instance.gameObject.transform;
                    inputCubeVector[i].layer = 2;
                    inputCubeVector[i].GetComponent<Renderer>().material.color = Color.white;
                }

                if (inputCube != null && inputCube.Count != 0)
                {
                    for (int i = 0; i < Instance.Knots.Count - 1; i++)
                    {
                        inputCube[i].transform.position = Instance.Knots[i].position;
                        inputCube[i].transform.rotation = Instance.Knots[i].rotation;

                        inputCubeVector[i].transform.position = Instance.Knots[i].position;
                        inputCubeVector[i].transform.rotation = Instance.Knots[i].rotation;
                    }
                }
                if (Instance.Knots.Count > inputCube.Count)
                {
                    inputCube.RemoveAt(inputCube.Count - 1);
                    inputCubeVector.RemoveAt(inputCube.Count - 1);
                }

                Debug.Log("Rendered");
            }
        }
        public class Serializer : ISerialize
        {
            Path Instance;

            public Serializer(Path instance)
            {
                this.Instance = instance;
            }

            public void ToXML()
            {

                //TODO �n�[�h�R�[�f�B���O����߂�
                var xml = new XDocument(
                     new XElement("Paths",
                          new XElement("Instance",
                               new XElement("Knots", Instance.Knots),
                               new XElement("LockAts", Instance.LookAts),
                               new XElement("Iteration", Instance.Iteration),
                               new XElement("IsLoop", Instance.IsLoop),
                               new XElement("Time", Instance.Time),
                               new XElement("IsCameraShake", Instance.IsCameraShake)
                          )
                     )
                );
                xml.Save(@"C:\Temp\LINQ_to_XML_Sample2.xml");
            }

            public void Serialize(string filename)
            {
                string text = System.IO.Path.Combine(ToolController.RecoveryDirectory, filename + ".xml");
                try
                {
                    if (!Directory.Exists(ToolController.RecoveryDirectory))
                    {
                        Directory.CreateDirectory(ToolController.RecoveryDirectory);
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
            public void Deserialize(string filename)
            {
                //var path = System.IO.Instance.Combine(ToolController.RecoveryDirectory, filename + ".xml");
                //if (File.Exists(path))
                //{
                //    List<CameraConfig> list = new List<CameraConfig>();
                //    try
                //    {
                //        using (FileStream fileStream = new FileStream(path, FileMode.Open))
                //        {
                //            list = (new XmlSerializer(list.GetType()).Deserialize(fileStream) as List<CameraConfig>);
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Debug.LogException(e);
                //    }
                //
                //}
            }
        }
    }
}