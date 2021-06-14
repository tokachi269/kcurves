﻿using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;

namespace Assets
{
    public class CameraDirector : MonoBehaviour
    {
        [SerializeField]
        public Path path;
        public Rotate rotate;

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

            //var mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            //Debug.Log(mainCamObj.transform.localRotation.z);
            //Quaternion rotation = GameObject.Find("Main Camera").transform.localRotation;
            //rotation.z = +90.0f;
            //GameObject.FindGameObjectWithTag("MainCamera").transform.localRotation = rotation;

        }

    }
}
