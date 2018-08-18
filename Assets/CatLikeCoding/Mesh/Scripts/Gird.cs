using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  如何创建自己的mesh，首先需要创建一个简单的三角形网格。
///	 创建一个mesh需要MeshFilter和MeshRenderer 组件
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Gird : MonoBehaviour {

	public int xSize, ySize;

	// 对于x * y 的mesh 总共需要 (x + 1) * (y + 1) 个点
	private Vector3[] vertices;

	// 物体的mesh
	private Mesh mesh;

	// 在脚本awake的时候创建mesh
	void Awake(){
		StartCoroutine(Generate());
	}

	// 
	private IEnumerator Generate(){
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		vertices = new Vector3[(xSize + 1) * (ySize + 1)];
		// UV坐标
		Vector2[] uv = new Vector2[vertices.Length];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0, 0, -1f);
		// 创建Mesh
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Produce Gird";

		for(int i = 0, y = 0; y <= ySize; y++){
			for(int x = 0; x <= xSize; x++, i++){
				vertices[i] = new Vector3(x, y);
				uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
				tangents[i] = tangent;
				yield return wait;
			}
		}

		// 将创建的点赋给mesh
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.tangents = tangents;

		// 创建三角面，每个三角面是由三个点组成。
		// 当组成这个三角面的三个点是顺时针时，是z轴正面可见的，如果 组成的是逆时针， 则是z轴反面可见的。
		//               第一个三角形 
		//            0             0
		//          1   2         2   1
		//          逆时针         顺时针
		int[] triangles = new int[6 * xSize * ySize];
		
		for(int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++){
			for(int x = 0; x < xSize; x++, ti+= 6, vi++){
				triangles[ti] = vi;
				triangles[ti + 2] = triangles[ti + 3] = vi + 1;
				triangles[ti + 1] = triangles[ti + 4] = xSize + vi + 1;
				triangles[ti + 5] = xSize + vi + 2;
				mesh.triangles = triangles;
				yield return wait;
			}
		}

		// 计算法线
		mesh.RecalculateNormals();
	}

	// 这个方法会被Unity Editor 自动调用， 用来画出Gizmos
	// Gizmos 是编辑模式的可视化提示. Gizmos工具可以画 icons, lines, etc.
	private void OnDrawGizmos(){
		// 该方法会在非运行模式下也调用，因此需要判断点是否为空。
		if(vertices == null) return;

		Gizmos.color = Color.black;
		for(int i = 0; i < vertices.Length; i++){
			Gizmos.DrawSphere(vertices[i], 0.1f);
			// 这些点是直接画在世界空间中的，所以不会随着这个GameObject的移动而移动，如果你想让这些点相对物体移动，你需要调用transform.TransformPoint(vertices[i]);
			// Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.1f);
		}
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
