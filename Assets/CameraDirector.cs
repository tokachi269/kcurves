using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;


namespace Assets
{
    public class CameraDirector : MonoBehaviour
    {
        public Path path;

        public bool isLoop = true;
        public bool iscameraShake = false;
        public bool isGaze = false;
        public bool isEquallySpaced = false;
        public bool isStart = false;
        [DefaultValue(4)]
        public  int iteration = 4;
        [SerializeField]
        public int time = 10;
        public float currentTime = 0;

        //セグメントごとの分割数
        public static int step = 10;
        public static GameObject[] inputCube;
        public static GameObject moveCameraCube;
        public static LineRenderer render;
        private int segIndex = 0;
        private float inputL = 0f;
        private float maxSpeed = 0f;

        public float dist = 0;
        public float distall = 0;
        public float diffT = 0;
        private Vector3 bef = Vector3.zero;
        private float befT = 0;

        void Start()
        {
            path = new Path();

            this.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), 60, false);
            this.AddKnot(new Vector3(1, 2, 1), new Quaternion(0, 0, 0, 0), 60, false);
            this.AddKnot(new Vector3(0, 2, 5), new Quaternion(0, 0, 0, 0), 60, true);
            this.AddKnot(new Vector3(0, -2, 1), new Quaternion(0, 0, 0, 0), 60, false);
            this.AddKnot(new Vector3(0, 1, 4), new Quaternion(0, 0, 0, 0), 60, false);
            this.AddLookAt(new Vector3(0, 4, 0), new Quaternion(0, 0, 0, 0), 60);

            render = this.gameObject.AddComponent<LineRenderer>();

            path.SetBezierFromKnots();
            moveCameraCube = new GameObject();
            moveCameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moveCameraCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            moveCameraCube.GetComponent<Renderer>().material.color = Color.blue;
            moveCameraCube.transform.position = path.Knots[0].position;
            moveCameraCube.transform.parent = this.transform;
            // moveCameraCube.AddComponent(typeof(JitterMotion));
            renderObject();
        }

        void OnValidate()
        {
            renderObject();
        }

        void Update()
        {
            //var mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            //Debug.Log(mainCamObj.transform.localRotation.z);
            //Quaternion rotation = GameObject.Find("Main Camera").transform.localRotation;
            //rotation.z = +90.0f;
            //GameObject.FindGameObjectWithTag("MainCamera").transform.localRotation = rotation;
            if (isStart)
            {
                path.extendBezierControls.CalcTotalKnotsLength();
                    //var mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
                    //Debug.Log(mainCamObj.transform.localRotation.z);
                    //Quaternion rotation = GameObject.Find("Main Camera").transform.localRotation;
                    //rotation.z = +90.0f;
                    //GameObject.FindGameObjectWithTag("MainCamera").transform.localRotation = rotation;

                if (path.Knots.Count > 1 && moveCameraCube != null)
                    {
                        maxSpeed = path.TotalLength / time * 0.01f;
                        float t = path.extendBezierControls.GetT(ref segIndex, ref inputL);

                        //inputL += currentTime / time / bezierResult.TotalLength;
                        Debug.Log("seg:" + segIndex + "  inputL:" + inputL + "maxS" + maxSpeed);
                        {
                            diffT = t - befT;
                            //Debug.Log("t:" + diffT);
                            befT = t;
                            Vector3 now = path.CalcPosition(isLoop, t);

                            distall += dist;
                            dist = Vector3.Distance(bef, now);
                            bef = now;
                            //Debug.Log("dist:" + dist);
                        }

                        moveCameraCube.transform.position = path.CalcPosition(isLoop, t);
                        //moveCameraCube.transform.rotation = path.CalcRotation(segIndex, inputL);

                        int i = (int)Math.Floor(currentTime / time * path.extendBezierControls.SegmentCount);

                        float dt = Time.deltaTime;
                        inputL += maxSpeed * dt;

                        currentTime += (0.01f / time);

                        if (currentTime > time) currentTime = 0f;
                    }

                

            }

        }

        public void renderObject()
        {
            if (!(path is null))
            {
                //TODO output出力
                var output = path.Output(step, isLoop);

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
                            inputCube[i].transform.position = path.Knots[i].position;
                            inputCube[i].transform.rotation = path.Knots[i].rotation;
                        }
                    }
                    else
                    {
                        inputCube = new GameObject[path.Knots.Count];

                        for (int i = 0; i < path.Knots.Count; i++)
                        {
                            GameObject.Destroy(inputCube[i]);
                            inputCube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            inputCube[i].transform.position = path.Knots[i].position;
                            inputCube[i].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                            inputCube[i].transform.parent = this.transform;

                            inputCube[i].GetComponent<Renderer>().material.color = Color.blue;

                        }
                    }
                
            }

            

        }

        public void AddKnot(Vector3 position, Quaternion rotation, float fov, bool lookAt)
        {
            path.Knots.Add(new ControlPoint(position, rotation, fov, lookAt));
        }

        public void AddLookAt(Vector3 position, Quaternion rotation, float fov)
        {
            path.LookAts.Add(new ControlPoint(position, rotation, fov, false));
        }


        public static float EaseInOutSine(float t){
            return (float)(-(Math.Cos(Math.PI * t) - 1) / 2);

        }

}
}
