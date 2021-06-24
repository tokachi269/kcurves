﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
	public class Bezier3D : MonoBehaviour
	{
		public Vector3 start = new Vector3(0, 0, 0);
		public Vector3 end = new Vector3(1, 1, 0);
		public Vector3 handle1 = new Vector3(0, 1, 0);
		public int resolution = 12;
		public float thickness = 0.25f;
		MeshFilter MeshFilter;
		public void Start()
		{
			gameObject.AddComponent<MeshRenderer>();
			MeshFilter = gameObject.AddComponent<MeshFilter>();
			MeshFilter.GetComponent<MeshRenderer>().material.color = Color.red;
			MeshFilter.mesh = CreateMesh();
		}

		public Mesh CreateMesh()
		{
			Mesh mesh = new Mesh();

			float scaling = 1;
			float width = thickness / 2f;
			List<Vector3> vertList = new List<Vector3>();
			List<int> triList = new List<int>();
			List<Vector2> uvList = new List<Vector2>();
			Vector3 upNormal = new Vector3(0, 0, -1);

			triList.AddRange(new int[] {
				2, 1, 0,    //start face
				0, 3, 2
			});

			for (int s = 0; s < resolution; s++)
			{
				float t = ((float)s) / resolution;
				float futureT = ((float)s + 1) / resolution;

				Vector3 segmentStart = BezierUtil.Position(start, handle1, end, t);
				Vector3 segmentEnd = BezierUtil.Position(start, handle1, end, futureT);

				Vector3 segmentDirection = segmentEnd - segmentStart;
				if (s == 0 || s == resolution - 1)
					segmentDirection = new Vector3(0, 1, 0);
				segmentDirection.Normalize();
				Vector3 segmentRight = Vector3.Cross(upNormal, segmentDirection);
				segmentRight *= width;
				Vector3 offset = segmentRight.normalized * (width / 2) * scaling;
				Vector3 br = segmentRight + upNormal * width + offset;
				Vector3 tr = segmentRight + upNormal * -width + offset;
				Vector3 bl = -segmentRight + upNormal * width + offset;
				Vector3 tl = -segmentRight + upNormal * -width + offset;

				int curTriIdx = vertList.Count;

				Vector3[] segmentVerts = new Vector3[]
				{
				segmentStart + br,
				segmentStart + bl,
				segmentStart + tl,
				segmentStart + tr,
				};
				vertList.AddRange(segmentVerts);

				Vector2[] uvs = new Vector2[]
				{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector2(1, 1)
				};
				uvList.AddRange(uvs);

				int[] segmentTriangles = new int[]
				{
				curTriIdx + 6, curTriIdx + 5, curTriIdx + 1, //left face
				curTriIdx + 1, curTriIdx + 2, curTriIdx + 6,
				curTriIdx + 7, curTriIdx + 3, curTriIdx + 0, //right face
				curTriIdx + 0, curTriIdx + 4, curTriIdx + 7,
				curTriIdx + 1, curTriIdx + 5, curTriIdx + 4, //top face
				curTriIdx + 4, curTriIdx + 0, curTriIdx + 1,
				curTriIdx + 3, curTriIdx + 7, curTriIdx + 6, //bottom face
				curTriIdx + 6, curTriIdx + 2, curTriIdx + 3
				};
				triList.AddRange(segmentTriangles);

				// final segment fenceposting: finish segment and add end face
				if (s == resolution - 1)
				{
					curTriIdx = vertList.Count;

					vertList.AddRange(new Vector3[] {
					segmentEnd + br,
					segmentEnd + bl,
					segmentEnd + tl,
					segmentEnd + tr
				});

					uvList.AddRange(new Vector2[] {
						new Vector2(0, 0),
						new Vector2(0, 1),
						new Vector2(1, 1),
						new Vector2(1, 1)
					}
					);
					triList.AddRange(new int[] {
					curTriIdx + 0, curTriIdx + 1, curTriIdx + 2, //end face
					curTriIdx + 2, curTriIdx + 3, curTriIdx + 0
				});
				}
			}

			mesh.vertices = vertList.ToArray();
			mesh.triangles = triList.ToArray();
			mesh.uv = uvList.ToArray();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.Optimize();

			return mesh;
		}
	}
}
