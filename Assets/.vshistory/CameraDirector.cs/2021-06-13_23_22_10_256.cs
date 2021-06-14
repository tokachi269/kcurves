using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;
using Assets.Mode;

namespace Assets
{
    public class CameraDirector : MonoBehaviour
    {
        [SerializeField]
        public Path path;
        public Rotate rotate;

        public bool isLoop = false;
        public bool iscameraShake = false;
        public bool isGaze = false;
        public bool isEquallySpaced = false;

        [DefaultValue(4)]
        public  int iteration = 4;
        
        [SerializeField]
        //セグメントごとの分割数
        public static ushort step = 10;

        void Start()
        {
            path = gameObject.AddComponent<Path>();
            rotate = gameObject.AddComponent<Rotate>();

            path.AddKnot(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), 60);
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


            path.Time = 10;

            path.SetBezierFromKnots();

            path.Render();
        }

        void OnValidate()
        {
            if (path!=null)
            {
                path.Render();
            }

        }

        void Update()
        {
            if (Input.GetKeyDown("k"))
            {
                path.AddKnot(CameraUtil.CameraPosition());
                Debug.Log("Add Knot Succeed");
                path.Render();
            }
            if (Input.GetKeyDown("j"))
            {
                path.RemoveKnot();
                Debug.Log("Remove Knot Succeed");
                path.Render();
            }
            if (Input.GetKeyDown("t"))
            {
                if (path.KnotsCount > 1)
                {
                    StartCoroutine(path.Play());
                }
                Debug.Log("Path Start!!");
            }

            if (Input.GetKeyDown("t"))
            {
                if (path.KnotsCount > 1)
                {
                    StartCoroutine(path.Play());
                }
                Debug.Log("Rorate Start!!");
            }
            //var mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            //Debug.Log(mainCamObj.transform.localRotation.z);
            //Quaternion rotation = GameObject.Find("Main Camera").transform.localRotation;
            //rotation.z = +90.0f;
            //GameObject.FindGameObjectWithTag("MainCamera").transform.localRotation = rotation;

        }

    }
}
