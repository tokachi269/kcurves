﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class BezierControls : MonoBehaviour
    {
        //ベジェ制御点群
        //c_{0,0}, c_{0,1}, c_{1,0}, ..., c_{n-1,0}, c_{n-1,1}, c_{n-1,2}の順
        public Vector3[] Points { get; set; }

        //セグメント数
        public int SegmentCount { get; set; }
        public double[] Ts { get; set; }
        //c_{i,j}
        public Vector3 this[int i, int j]
        {
            get 
            {
                return Points[2 * i + j];
            }
            set
            {
                Points[2 * i + j] = value;
            }
        }

        //コンストラクタ
        public BezierControls(int n, bool isLoop)
        {
            SegmentCount = n < 3 ? 1 : n;
            Points = new Vector3[2 * SegmentCount + 1];
            Ts = new double[SegmentCount];
        }
    }
}
