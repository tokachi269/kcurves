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

        //���[�U�[����_
        protected override List<ControlPoint> Knots { get; set; } = new List<ControlPoint>();
        public int KnotsCount => Knots.Count;

        //���[�U�[���䒍���_
        protected List<ControlPoint> LookAts { get; private set; } = new List<ControlPoint>();
        public int LookAtsCount => Knots.Count;

        //Bezier�v�Z����
        protected ExtendBezierControls Beziers { get; private set; }
        public int BeziersCount => Beziers.SegmentCount;
        public int BeziersPointsLength => Beziers.Points.Length;
        public float TotalLength => Beziers.TotalLength;
        
        private int Iteration = 5;
        [SerializeField]
        public bool IsLoop= false;
        [SerializeField]
        public ushort Time = 10;
        [SerializeField]
        public ushort Step = 10;

        [SerializeField]
        public bool IsCameraShake { get; set; }
        //public PerlinCameraShake CameraShake;

        protected ControlPoint DefaultPosition { get; set; }

        private GameObject moveCameraCube;

        public static List<GameObject> inputCube = new List<GameObject>();
        public List<GameObject> bezierObject = new List<GameObject>();

        public static LineRenderer render;

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
        public void Start()
        {
           // CameraShake = gameObject.AddComponent<PerlinCameraShake>();
           // CameraShake.enabled = false;
            moveCameraCube = new GameObject("moveCameraCube");
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.parent = this.transform;

            render = gameObject.AddComponent<LineRenderer>();
        }

        void OnValidate()
        {
            SetBezierFromKnots();
        }

        public void SetBezierFromKnots()
        {
            Beziers = KCurves.CalcBeziers(Knots, Iteration, IsLoop) as ExtendBezierControls;
            if (Beziers.SegmentCount >= 3)
            {
                int segment;
                Vector3[] divVec = DividePoints(IsLoop, out segment);
                Beziers = new ExtendBezierControls(segment, divVec, IsLoop);
            }

            Render();
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
                    Vector3 now = CalcPosition(bezierIndex, t, IsLoop);
                
                    distall += dist;
                    dist = Vector3.Distance(bef, now);
                    bef = now;
                    //Debug.Log("dist:" + dist);
                }

                if (t <= Beziers.SegmentCount)
                {
                    Vector3 pos = CalcPosition(bezierIndex, t, IsLoop);
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

        private Vector3 CalcPosition(int bezierIndex, float t, bool isLoop)
        {

            return BezierUtil.Position(Beziers[bezierIndex, 0], Beziers[bezierIndex, 1], Beziers[bezierIndex, 2], t % 1);
        }

        private Quaternion CalcRotation(int knotIndex, float ratio)
        {
            Vector3 rotation;

            float easing = Easing.EaseInOutSine(ratio);

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
                    rotation = Vector3.Lerp(Knots[knotIndex].rotation.eulerAngles, Knots[knotIndex + 1].rotation.eulerAngles, easing);

                }
            }
            else
            {
                rotation = Knots[knotIndex].rotation.eulerAngles;
            }


            if (LookAts.Count != 0)
            {
                //if (Knots[knotIndex].isLookAt || Knots[knotIndex+1].isLookAt)
                //{
                //    rotation = Vector3.Lerp((Knots[knotIndex].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation),
                //                           (Knots[knotIndex+1].isLookAt ? (Quaternion.LookRotation(LookAts[0].position) * new Quaternion(1, -1, 1, 1)).eulerAngles : rotation), ratio);
                //}
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

        // ���[�U�[����_���x�W�F����_�ɒǉ�����
        private Vector3[] DividePoints(bool isLoop, out int dividedSegmentCount)
        {
            Debug.Log("Beziers.SegmentCount:" + Beziers.SegmentCount);

            if (isLoop)
            {
                dividedSegmentCount = (Beziers.SegmentCount * 2 - 1);
            }
            else
            { 
                dividedSegmentCount = (Beziers.SegmentCount - 2) * 2;
            }
            
            Debug.Log("dividedSegmentCount:" + dividedSegmentCount);
            Vector3[] dividedPoints = new Vector3[isLoop ? dividedSegmentCount * 2 + 4 : dividedSegmentCount * 2 + 1];

            ushort segCnt = (ushort)(isLoop ? Beziers.SegmentCount : Beziers.SegmentCount - 1);
            Debug.Log("segCnt" + segCnt);

            var tss = Beziers.Ts.Select((num, index) => (num, index));
            foreach (var t in tss) Debug.Log(t.index+"," + t.num);

            ushort index = 0;
            int i = isLoop ? 0 : 1;
            for (; i < segCnt; i++)
            {
                Debug.Log("i=" + i +","+ Beziers[i, 0] +","+ Beziers[i, 1] + "," + Beziers[i, 2] + "," + (float)Beziers.Ts[i]);
                Vector3[] result = BezierUtil.Divide(Beziers[i, 0], Beziers[i, 1], Beziers[i, 2], (float)Beziers.Ts[i]);
                int condition = (i == segCnt - 1 && isLoop) ? 4 : 3;
                for (int j=0; j <= condition; j++,index++)
                {
                    Debug.Log("dividedPoints" + dividedPoints.Length+" index:"+index + ", "+result[j]);
                    dividedPoints[index] = result[j];
                }

            }
            if (!isLoop) dividedPoints[dividedPoints.Length - 1] = Beziers[segCnt,0];
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

        public void AddKnot(ControlPoint cp)
        {
            this.Knots.Add(cp);
            if (Knots.Count == 0)
            {
                moveCameraCube.transform.position = Knots[0].position;
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