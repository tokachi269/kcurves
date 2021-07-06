using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 線状のメッシュを描画
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteInEditMode]
public class LineRendererEx : MonoBehaviour {

	/// <summary>
	/// ノード記憶用クラス
	/// </summary>
	[Serializable]
	public class Node {
		public Vector3 pos;	//座標
		public Vector3 scale;	//大きさ
		public Color color;	//色
		//public Mesh mesh;	//メッシュ

		//Constructor
		public Node() {
			this.pos = Vector3.zero;
			scale = Vector3.one;
			color = new Color(1f, 0.5f, 0.2f, 0.5f);
		}
		public Node(Vector3 pos, Vector3 scale, Color color) {
			this.pos = pos;
			this.scale = scale;
			this.color = color;
		}
	}
	/// <summary>
	/// エッジ記憶用クラス(?)
	/// </summary>
	public class Edge {
		public Mesh mesh;	//メッシュ

		//Constructor
		public Edge() {
			
		}
	}

	[Header("Edge Parameter")]
	[SerializeField, Range(0.001f, 1f)]
	private float edgeSize = 1f;		//エッジの大きさ
	[SerializeField, Range(2, 16)]
	private int edgeSquare = 4;			//エッジの角形
	[Header("Node Parameter")]
	[SerializeField]
	private Mesh nodeMesh;				//ノードのメッシュ
	[SerializeField]
	private Vector3 defaultNodeScale = Vector3.one;	//デフォルトノードスケール
	[SerializeField]
	private Color defaultNodeColor = Color.white;	//デフォルトノードカラー
	[Header("Nodes")]
	[SerializeField]
	private List<Node> nodes;	//ノードリスト
	//内部パラメータ
	private MeshFilter mf; 		//表示用
	private List<Edge> edges;	//エッジリスト

#region MonoBehaviourEvent
	private void Awake() {
		if (mf == null) mf = GetComponent<MeshFilter>();
		if (nodes == null) nodes = new List<Node>();
		if (edges == null) edges = new List<Edge>();
		if (edges == null){
			nodeMesh = new Mesh();
			Vector3[] vertices = new Vector3[3];
			int[] triangles = new int[3] { 0, 1, 2 };
			for (int i = 0; i < 3; i++)
			{
				float angle = i * 120f * Mathf.Deg2Rad;
				vertices[i] = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
			}

			nodeMesh.vertices = vertices;
			nodeMesh.triangles = triangles;
			nodeMesh.RecalculateNormals();
			GetComponent<MeshFilter>().mesh = nodeMesh;
		}
	}
	private void OnValidate() {
		Awake();
	}
#endregion
#region Function
	/// <summary>
	/// 更新
	/// </summary>
	public void Apply() {
	    if(nodes.Count <= 0) return;
		//線状のメッシュを作成
		Mesh mesh = CreateLineMesh(nodes, edges);
		mesh.RecalculateNormals();
		mf.mesh = mesh;
	}
	/// <summary>
	/// 座標の追加
	/// </summary>
	public void AddPosition(Vector3 pos, Vector3 scale, Color color) {
		if (nodeMesh == null) return;
		//ノードの追加
		nodes.Add(new Node(pos, scale, color));
		//エッジの追加
		if(nodes.Count > 1) {
			edges.Add(new Edge());
		}
	}
	/// <summary>
	/// 座標の追加
	/// </summary>
	public void AddPosition(Vector3 pos) {
		AddPosition(pos, defaultNodeScale, defaultNodeColor);
	}
	/// <summary>
	/// 座標の挿入
	/// </summary>
	public void InsertPosition(int index, Vector3 pos, Vector3 scale, Color color) {
		// if(nodeMesh == null) return;
		//ノードの挿入
		nodes.Insert(index, new Node(pos, scale, color));
		//エッジの挿入
		if(nodes.Count > 1) {
			edges.Insert(index, new Edge());
		}
	}
	/// <summary>
	/// 座標の挿入
	/// </summary>
	public void InsertPosition(int index, Vector3 pos) {
		InsertPosition(index, pos, defaultNodeScale, defaultNodeColor);
	}
	/// <summary>
	/// 座標の削除
	/// </summary>
	public void RemoveAtPosition(int index) {
		if(index < 0 || nodes.Count <= index) return;
		//ノードの削除
		nodes.RemoveAt(index);
		//エッジの削除,修正
		if(edges.Count > 0) {
			if(nodes.Count == index) {
				edges.RemoveAt(index - 1);
			} else {
				edges.RemoveAt(index);
			}
		}
	}
	/// <summary>
	/// 末尾の座標の削除
	/// </summary>
	public void RemoveLastPosition() {
		RemoveAtPosition(nodes.Count - 1);
	}
	/// <summary>
	/// 初期化
	/// </summary>
	public void Clear() {
		mf.mesh.Clear();
		nodes.Clear();
		edges.Clear();
	}
#endregion
#region Mesh
	/// <summary>
	/// 線状のメッシュを作成
	/// </summary>
	private Mesh CreateLineMesh(List<Node> nodes, List<Edge> edges) {
		//メッシュのリストを作成
		List<Mesh> meshs = new List<Mesh>();
		//最初のノード
		meshs.Add(CongigureMesh(CopyMesh(nodeMesh), nodes[0].pos, nodes[0].scale, nodes[0].color));
		for(int i = 1; i < nodes.Count; ++i) {
			//エッジ
			meshs.Add(CreateEdgeMesh(nodes[i - 1], nodes[i], edgeSize, edgeSquare));
			//ノード
			meshs.Add(CongigureMesh(CopyMesh(nodeMesh), nodes[i].pos, nodes[i].scale, nodes[i].color));
		}
		//メッシュの結合
		return ConnectMesh(meshs);
	}
	/// <summary>
	/// メッシュの結合
	/// </summary>
	private Mesh ConnectMesh(List<Mesh> meshs) {
		int vertexCount = meshs.Sum(elem => elem.vertexCount);
		Vector3[] vertices = new Vector3[vertexCount];
		Vector2[] uvs = new Vector2[vertexCount];
		Color[] colors = new Color[vertexCount];
		int triangleCount = meshs.Sum(elem => elem.triangles.Length);
		int[] triangles = new int[triangleCount];

		//結合処理
		int vertexOffset = 0, triangleOffset = 0, index;
		Vector3[] tempVerts;
		Vector2[] tempUvs;
		Color[] tempColors;
		int[] tempTris;
		for(int i = 0; i < meshs.Count; ++i) {
			//vertex, uv, color
			tempVerts = meshs[i].vertices;
			tempUvs = meshs[i].uv;
			tempColors = meshs[i].colors;
			for(int j = 0; j < meshs[i].vertexCount; ++j) {
				index = j + vertexOffset;
				vertices[index] = tempVerts[j];
				uvs[index] = tempUvs[j];
				colors[index] = tempColors[j];
			}
			//triangle
			tempTris = meshs[i].triangles;
			for(int j = 0; j < meshs[i].triangles.Length; ++j) {
				triangles[j + triangleOffset] = tempTris[j] + vertexOffset;
			}
			vertexOffset += meshs[i].vertexCount;
			triangleOffset += meshs[i].triangles.Length;
		}
		//メッシュに落とし込む
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.colors = colors;
		mesh.triangles = triangles;
		return mesh;
	}
	/// <summary>
	/// メッシュの設定
	/// </summary>
	private Mesh CongigureMesh(Mesh mesh, Vector3 translate, Vector3 scale, Color color) {
		Color[] colors = new Color[mesh.vertexCount];
		Vector3[] vertices = mesh.vertices;
		Matrix4x4 transMat = Matrix4x4.TRS(translate, Quaternion.identity, scale);
		for(int i = 0; i < mesh.vertexCount; ++i) {
			vertices[i] = transMat.MultiplyPoint(vertices[i]);
			colors[i] = color;
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		return mesh;
	}
	/// <summary>
	/// メッシュのコピー
	/// </summary>
	private Mesh CopyMesh(Mesh src) {
		Mesh dst = new Mesh();
		dst.vertices = src.vertices;
		dst.triangles = src.triangles;
		dst.uv = src.uv;
		dst.normals = src.normals;
		dst.colors = src.colors;
		dst.tangents = src.tangents;
		return dst;
	}
#endregion
#region EdgeMesh
	/// <summary>
	/// エッジメッシュの作成
	/// </summary>
	private Mesh CreateEdgeMesh(Vector3 from, Color fromColor, Vector3 to, Color toColor, float size, int square = 4) {
		Vector3 dir = (to - from).normalized;						//方向ベクトル
		//Quaternion.LookRotation(dir)がみそ
		Vector3 dirVertical = Quaternion.AngleAxis(90, dir) * (Quaternion.LookRotation(dir) * Vector3.right) * size;	//方向ベクトルに垂直なベクトル

		Vector3[] vertices = new Vector3[square * 4];
		Vector2[] uvs = new Vector2[square * 4];
		Color[] colors = new Color[square * 4];
		int[] triangles = new int[square * 6];
		for (int i = 0; i < square; ++i) {
			Vector3 angleDir1 = Quaternion.AngleAxis((360f / square) * i, dir) * dirVertical;
			Vector3 angleDir2 = Quaternion.AngleAxis((360f / square) * (i + 1), dir) * dirVertical;
			//vertex
			vertices[i * 4 + 0] = from + angleDir1;
			vertices[i * 4 + 1] = to + angleDir1;
			vertices[i * 4 + 2] = from + angleDir2;
			vertices[i * 4 + 3] = to + angleDir2;
			//uv
			uvs[i * 4 + 0] = new Vector2(0f, 0f);
			uvs[i * 4 + 1] = new Vector2(1f, 0f);
			uvs[i * 4 + 2] = new Vector2(0f, 1f);
			uvs[i * 4 + 3] = new Vector2(1f, 1f);
			//Color
			colors[i * 4 + 0] = fromColor;
			colors[i * 4 + 1] = toColor;
			colors[i * 4 + 2] = fromColor;
			colors[i * 4 + 3] = toColor;
			//triangles
			triangles[i * 6 + 0] = i * 4 + 0;
			triangles[i * 6 + 1] = i * 4 + 2;
			triangles[i * 6 + 2] = i * 4 + 1;

			triangles[i * 6 + 3] = i * 4 + 2;
			triangles[i * 6 + 4] = i * 4 + 3;
			triangles[i * 6 + 5] = i * 4 + 1;
		}
		Mesh m = new Mesh();
		m.vertices = vertices;
		m.uv = uvs;
		m.colors = colors;
		m.triangles = triangles;
		return m;
	}
	/// <summary>
	/// エッジメッシュの作成
	/// </summary>
	private Mesh CreateEdgeMesh(Node from, Node to, float size, int square = 4) {
		return CreateEdgeMesh(from.pos, from.color, to.pos, to.color, size, square);
	}
#endregion
}