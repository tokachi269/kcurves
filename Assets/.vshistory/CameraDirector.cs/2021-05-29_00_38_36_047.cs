using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEditor;

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


        //セグメントごとの分割数
        public static int step = 10;
        //TODO List型に変更
        public static List<GameObject> inputCube = new List<GameObject>();
        public List<GameObject> bezierObject = new List<GameObject>();

        public static GameObject moveCameraCube;
        public static LineRenderer render;

        void Start()
        {
            AddTag("moveCameraCube");
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
            moveCameraCube = new GameObject("moveCameraCube");
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
                if (path.Knots.Count > 1 && moveCameraCube != null)
                {
                    path.MoveCamera();
                }
                //moveCameraCube.transform.position = pos;
                //moveCameraCube.transform.rotation = rot;
            }
        }

        static void AddTag(string tagname)
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tagname)
                    {
                        return;
                    }
                }

                int index = tags.arraySize;
                tags.InsertArrayElementAtIndex(index);
                tags.GetArrayElementAtIndex(index).stringValue = tagname;
                so.ApplyModifiedProperties();
                so.Update();
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

    }
}
