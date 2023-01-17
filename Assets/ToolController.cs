using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;

namespace CamOpr.Tool
{
    public class ToolController : MonoBehaviour
    {
        [SerializeField]
        public PathTool path;

        [SerializeField]
        public RotateTool rotate;
        public static string RecoveryDirectory => System.IO.Path.Combine(Directory.GetCurrentDirectory(), "CameraOperator");
        
        public Dictionary<string, BaseCameraMode> saves;
        void Start()
        {
            path = gameObject.AddComponent<PathTool>();
            rotate = gameObject.AddComponent<RotateTool>();
/*
            path.AddKnot(new Vector3(0, 0, 1), new Quaternion(0, 0, 0, 1), 60);
            path.AddKnot(new Vector3(1, 1, 1), new Quaternion(0, 0, 0, 1), 60);
            path.AddKnot(new Vector3(0, 2, 5), new Quaternion(0, 0, 0, 1), 60);
            path.AddKnot(new Vector3(0, -2, 1), new Quaternion(0, 0, 0, 1), 60);
            path.AddKnot(new Vector3(5, 1, 1), new Quaternion(0, 0, 0, 1), 60);

            //path.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), 60);
            //path.AddKnot(new Vector3(0, 1, 1), new Quaternion(0, 0, 0, 1), 60);
            //path.AddKnot(new Vector3(0, 2, 0), new Quaternion(0, 0, 0, 1), 60);
            //path.AddKnot(new Vector3(0, 3, 3), new Quaternion(0, 0, 0, 1), 60);
            //path.AddKnot(new Vector3(0, 4, 0), new Quaternion(0, 0, 0, 1), 60);
            path.AddLookAt(new Vector3(0, 4, 0), new Quaternion(0, 0, 0, 1), 60);
*/
            rotate.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), 60);

            saves = new Dictionary<string, BaseCameraMode>();
        }


        void Update()
        {
            Vector3 pos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit hit;
            if (Input.GetKey("h"))
            {
                path.GetCursorPositionPath();

                if (Input.GetMouseButtonDown(0))
                {
                    path.AddKnotMiddle();
                }
            }
            if (Input.GetKey("f"))
            {
                path.MoveKnot();

                if (Input.GetMouseButtonDown(0))
                {
                    path.AddKnotMiddle();
                }
            }

            if (Input.GetKeyDown("k"))
            {
                path.AddKnot(CameraUtils.CameraPosition());
                Debug.Log("Add Knot Succeed");
                path.Display();
            }
            if (Input.GetKeyDown("j"))
            {
                path.RemoveKnot();
                Debug.Log("Remove Knot Succeed");
                path.Display();
            }
            if (Input.GetKeyDown("t"))
            {
                if (path != null)
                {
                    StartCoroutine(path.Play());
                }
                Debug.Log("Path Start!!");
            }

            if (Input.GetKeyDown("y"))
            {
                if (rotate!= null)
                {
                    StartCoroutine(rotate.Play());
                }
                Debug.Log("Rotate Start!!");
            }
            if (Input.GetKeyDown("u"))
            {
                if (path != null)
                {
                    path.serializer.Serialize("xx");
                }
                Debug.Log("path Serialized");
            }
        }
    }
}
