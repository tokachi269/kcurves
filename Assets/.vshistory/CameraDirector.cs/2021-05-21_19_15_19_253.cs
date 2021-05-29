using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;


namespace Assets
{
    public class CameraDirector : MonoBehaviour
    {
        [SerializeField]
        public Path path;

        public bool isLoop = false;
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
        //TODO List型に変更
        public static List<GameObject> inputCube = new List<GameObject>();
        public List<GameObject> bezierObject = new List<GameObject>();

        public static GameObject moveCameraCube;
        public static LineRenderer render;
        public float t = 0;
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
            path = gameObject.AddComponent<Path>();
            path.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddKnot(new Vector3(1, 1, 1), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddKnot(new Vector3(0, 2, 5), new Quaternion(0, 0, 0, 1), 60, true);
            path.AddKnot(new Vector3(0, -2, 1), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddKnot(new Vector3(5, 1, 1), new Quaternion(0, 0, 0, 1), 60, false);

            //path.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), 60, false);
            //path.AddKnot(new Vector3(0, 1, 1), new Quaternion(0, 0, 0, 1), 60, false);
            //path.AddKnot(new Vector3(0, 2, 0), new Quaternion(0, 0, 0, 1), 60, true);
            //path.AddKnot(new Vector3(0, 3, 3), new Quaternion(0, 0, 0, 1), 60, false);
            //path.AddKnot(new Vector3(0, 4, 0), new Quaternion(0, 0, 0, 1), 60, false);
            path.AddLookAt(new Vector3(0, 4, 0), new Quaternion(0, 0, 0, 1), 60);

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
            if (Input.GetKeyDown("k"))
            {
                path.AddKnot(Camera.main.transform.position, Camera.main.transform.rotation, 60, false);
                Debug.Log("Add Knot Succeed");
                renderObject();
            }
            if (Input.GetKeyDown("j"))
            {
                path.RemoveKnot();
                Debug.Log("Remove Knot Succeed");
                renderObject();
            }
            //var mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            //Debug.Log(mainCamObj.transform.localRotation.z);
            //Quaternion rotation = GameObject.Find("Main Camera").transform.localRotation;
            //rotation.z = +90.0f;
            //GameObject.FindGameObjectWithTag("MainCamera").transform.localRotation = rotation;
            if (isStart)
            {
                //Debug.Log("TotalLength:" + path.Beziers.TotalLength);
                if (path.Knots.Count > 1 && moveCameraCube != null)
                {
                    maxSpeed = path.MaxSpeed(time);
                    t = path.Beziers.GetT(ref segIndex, ref inputL);
                    //Debug.Log(t);
                    //Debug.Log("seg:" + segIndex + "  inputL:" + inputL + "maxS:" + maxSpeed +"currentTime:"+ currentTime);
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

                    if (t <= path.Beziers.SegmentCount)
                    {
                        Vector3 pos= path.CalcPosition(isLoop, t);
                        Quaternion rot= path.CalcRotation(segIndex, inputL);
                        GameObject.Find("CameraRig").transform.position = pos;
                        GameObject.Find("CameraRig").transform.rotation = rot;
                        moveCameraCube.transform.position = pos;
                        moveCameraCube.transform.rotation = rot;
                    }

                    int i = (int)Math.Floor(currentTime / time * path.Beziers.SegmentCount);

                    float dt = Time.deltaTime;
                    inputL += maxSpeed * dt;

                    currentTime += dt;

                    if (currentTime >= time)
                    {
                        segIndex = 0;
                        currentTime = 0f;
                        inputL = 0f;
                        moveCameraCube.transform.position = path.Knots[0].position;
                    }
                }
            }
        }

        public void renderObject()
        {
            if (!(path is null))
            {
                //TODO output出力
                var output = path.Output(step, isLoop);

                for (int i = 0; i < bezierObject.Count; i++)
                {
                    Destroy(bezierObject[i]);
                }
                bezierObject.Clear();

                for (int i = 1; i < path.Beziers.SegmentCount-1; i++)
                {
                    bezierObject.Add(new GameObject("bezierControl" + i));
                    bezierObject[i - 1] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bezierObject[i - 1].transform.position = path.Beziers[i,0];
                
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
                for (int i = 0; i < path.Knots.Count; i++)
                    {
                        inputCube.Add(new GameObject("inputCube" + i));
                        inputCube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        inputCube[i].transform.position = path.Knots[i].position;
                        inputCube[i].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        inputCube[i].transform.parent = this.transform;
                        
                        inputCube[i].GetComponent<Renderer>().material.color = Color.blue;
                    }



                if (inputCube != null && inputCube.Count != 0)
                {
                    for (int i = 0; i < path.Knots.Count - 1; i++)
                    {
                        inputCube[i].transform.position = path.Knots[i].position;
                        inputCube[i].transform.rotation = path.Knots[i].rotation;
                    }
                }
                if (path.Knots.Count > inputCube.Count)
                {
                    inputCube.RemoveAt(inputCube.Count - 1);
                }

            }
            Debug.Log("Rendered");
        }


        public static float EaseInOutSine(float t){
            return (float)(-(Math.Cos(Math.PI * t) - 1) / 2);
        }
        public static float easeInSine(float t) {
            return ((float)1 - Math.Cos((Math.PI * t) / 2));

    }

}
}
