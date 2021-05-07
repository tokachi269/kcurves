using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;


namespace Assets
{
    public class CameraDirector : MonoBehaviour
    {
        public static KCurves kCurves;
        BezierControls bezierResult;

        public bool isLoop = true;
        public bool iscameraShake = false;
        public bool isGaze = false;
        public bool isEquallySpaced = false;
        
        [DefaultValue(4)]
        public  int iteration = 4;
        [SerializeField]
        public int time = 10;
        public float currentTime = 0;

        //セグメントごとの分割数
        public static int step = 100;
        public static GameObject[] inputCube;
        public static GameObject moveCameraCube;
        public static LineRenderer render;

        [SerializeField]
        //ユーザー制御点群
        public List<Knot> Knots = new List<Knot>();

        public void AddKnot(Vector3 position, Quaternion rotation, float fov)
        {
            this.Knots.Add(new Knot(position, rotation, fov));
        }


        public float dist = 0;
        public float distall = 0;
        public float diffT = 0;
        private Vector3 bef = Vector3.zero;
        private float befT = 0;

        void Start()
        {
            
            this.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), 60);
            this.AddKnot(new Vector3(1, 1, 1), new Quaternion(0, 0, 0, 0), 60);
            this.AddKnot(new Vector3(0, 2, 0), new Quaternion(1, 1, 1, 0), 60);
            this.AddKnot(new Vector3(0, -2, 1), new Quaternion(0, 0, 0, 0), 60);
            this.AddKnot(new Vector3(0, 1, 4), new Quaternion(0, 0, 0, 0), 60);
            
            render = this.gameObject.AddComponent<LineRenderer>();
            
            kCurves = this.gameObject.AddComponent<KCurves>();
            xx();
            moveCameraCube = new GameObject();
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.position = Knots[0].position;
            moveCameraCube.transform.parent = this.transform;
            // moveCameraCube.AddComponent(typeof(JitterMotion));
        }
        void Update()
        {
            //var mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            //Debug.Log(mainCamObj.transform.localRotation.z);
            //Quaternion rotation = GameObject.Find("Main Camera").transform.localRotation;
            //rotation.z = +90.0f;
            //GameObject.FindGameObjectWithTag("MainCamera").transform.localRotation = rotation;

            if (bezierResult.Knots.Count > 1 && moveCameraCube != null)
            {   
                float t = bezierResult.GetT(currentTime / time * bezierResult.TotalLength);

                {
                    diffT = t - befT;
                    Debug.Log("t:" + diffT);
                    befT = t;
                    Vector3 now = bezierResult.CalcPosition(isLoop, t);

                    distall += dist;
                    dist = Vector3.Distance(bef, now);
                    bef = now;
                    Debug.Log("dist:" + dist);
                }
                
                moveCameraCube.transform.position = bezierResult.CalcPosition(isLoop, t);

                int i = (int)Math.Floor(currentTime / time * bezierResult.SegmentCount);

                float dt = Time.deltaTime;
                currentTime += (0.01f / time);
                    
                    if (currentTime > time) currentTime = 0f;
            }

        }
        void OnValidate()
        {
            xx();
        }
        public void xx()
        {
            if (CameraDirector.kCurves != null)
            {
               
                bezierResult = CameraDirector.kCurves.CalcBezier(Knots, isLoop);
                bezierResult.Knots = this.Knots;

                // plotsを計算
                bezierResult.CalcPlots(step, isLoop, isEquallySpaced);
                bezierResult.CalcArcLengthWithT(isLoop);
                bezierResult.CalcKnotsLength(isLoop);
                bezierResult.CalcTotalKnotsLength();

                //tの移動距離を計算し、パラメータ化する
                bezierResult.CalcArcLengthWithT(isLoop);

                //TODO output出力
                var output = bezierResult.CalcPlots(step, isLoop, isEquallySpaced);

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


                if (inputCube != null && inputCube.Length != 0)
                {
                    for (int i = 0; i < inputCube.Length; i++)
                    {
                        inputCube[i].transform.position = Knots[i].position;
                    }
                }else
                {
                    inputCube = new GameObject[Knots.Count];

                    for (int i = 0; i < Knots.Count; i++)
                    {
                        GameObject.Destroy(inputCube[i]);
                        inputCube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        inputCube[i].transform.position = Knots[i].position;
                        inputCube[i].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        inputCube[i].transform.parent = this.transform;

                        inputCube[i].GetComponent<Renderer>().material.color = Color.blue;

                    }
                }
            }

        }



        public static float[] CalcIntegralBezierLength(BezierControls cs, bool isLoop)
        {
            float[] length = new float[cs.SegmentCount];
            int  k;
            int segCnt = isLoop || cs.SegmentCount < 3 ? cs.SegmentCount : cs.SegmentCount - 2;
            for (k = 0; k < segCnt; k++)
            {
                length[k] = BezierUtil.CalcBezierLength(cs[k, 0], cs[k, 1], cs[k, 2],1);
            }
            var last = isLoop || cs.SegmentCount < 3 ? 0 : k;
            length[last] = BezierUtil.CalcBezierLength(cs[last, 0], cs[last, 1], cs[last, 2],1);
            return length;
        }



    }
}
