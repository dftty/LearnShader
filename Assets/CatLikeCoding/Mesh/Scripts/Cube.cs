using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour {

	public int xSize, ySize, zSize;

	// 圆角化距离
	public int roundness;

	private Mesh mesh;

	private Vector3[] vertices;

	private Vector3[] normals;

	/// <summary>
	///  生成两个三角面的方法
	/// </summary>
	private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11){
		triangles[i] = v00;
		triangles[i + 1] = triangles[i + 4] = v01;
		triangles[i + 2] = triangles[i + 3] = v10;
		triangles[i + 5] = v11;
		return i + 6;
	}

	void Awake(){
		StartCoroutine(Generate());
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator Generate(){
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Producedural Cube";
		WaitForSeconds wait = new WaitForSeconds(0.1f);

		yield return StartCoroutine(CreateVertices());
		CreateTriangles();


		yield return wait;
	}

	IEnumerator CreateVertices(){
		int conerVertices = 8;
		int edgeVertices = (xSize + ySize + zSize - 3) *4;
		int faceVertices = ((xSize - 1) * (ySize - 1) + (ySize - 1) * (zSize - 1) + (xSize - 1) * (zSize - 1)) * 2;
		vertices = new Vector3[conerVertices + edgeVertices + faceVertices];
		normals = new Vector3[vertices.Length];

		WaitForSeconds wait = new WaitForSeconds(0.1f);

		int v = 0;
		// 添加四周的点
		for(int y = 0; y <= ySize; y++){
			for(int x = 0; x <= xSize; x++){
				SetVertex(v++, x, y, 0);
				yield return wait;
			}

			for(int z = 1; z <= zSize; z++){
				SetVertex(v++, xSize, y, z);
				yield return wait;
			}

			for(int x = xSize - 1; x >= 0; x--){
				SetVertex(v++, x, y, zSize);
				yield return wait;
			}

			for(int z = zSize - 1; z > 0; z--){
				SetVertex(v++, 0, y, z);
			}
		}

		
		// 添加上下点
		for(int z = 1; z < zSize; z++){
			for(int x = 1; x < xSize; x++){
				SetVertex(v++, x, ySize, z);
				yield return wait;
			}
		}

		for(int z = 1; z < zSize; z++){
			for(int x = 1; x < xSize; x++){
				SetVertex(v++, x, 0, z);
				yield return wait;
			}
		}

		

		mesh.vertices = vertices;
		mesh.normals = normals;
	}

	private void SetVertex(int i, int x, int y, int z){
		Vector3 inner = vertices[i] = new Vector3(x, y, z);

		// 
		if(x < roundness){
			inner.x = roundness;
		}else if(x > xSize - roundness){
			inner.x = xSize - roundness;
		}
		
		if(y < roundness){
			inner.y = roundness;
		}else if(y > ySize - roundness){
			inner.y = ySize - roundness;
		}

		if(z < roundness){
			inner.z = roundness;
		}else if(z > zSize - roundness){
			inner.z = zSize - roundness;
		}
 
		normals[i] = (vertices[i] - inner).normalized;
		vertices[i] = inner + normals[i] * roundness;
	}

	// 创建所有的面
	void CreateTriangles(){
		// 将每个轴的面组成一个数组，
		int[] trianglesZ = new int[(xSize * ySize) * 12];
		int[] trianglesX = new int[(ySize * zSize) * 12];
		int[] trianglesY = new int[(xSize * zSize) * 12];


		int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
		int[] triangles = new int[quads * 6];
		int ring = (xSize + zSize) * 2;
		int tZ = 0, tX = 0, tY = 0, v = 0;

		// 创建前后左右的四个面
		for(int y = 0; y < ySize; y++, v++){
			for(int q = 0; q < xSize; q++, v++){
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for(int q = 0; q < zSize; q++, v++){
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			for(int q = 0; q < xSize; q++, v++){
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for(int q = 0; q < zSize - 1; q++, v++){
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
		}

		tY = CreateTopFace(trianglesY, tY, ring);
		tY = CreateBottomFace(trianglesY, tY, ring);

		//mesh.triangles = triangles;
		mesh.subMeshCount = 3;
		mesh.SetTriangles(trianglesZ, 0);
		mesh.SetTriangles(trianglesX, 1);
		mesh.SetTriangles(trianglesY, 2);
	}

	/// <summary>
	///  创建顶部的面
	/// </summary>
	private int CreateTopFace(int[] triangles, int t, int ring){
		int v = ring * ySize;
		// 第一行
		for(int x = 0; x < xSize - 1; x++, v++){
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		// 中间行
		int vMin = ring * (ySize + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;
		for(int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++){
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
			for(int x = 1; x < xSize - 1; x++, vMid++){
				t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
			}
			t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
		}

		// 最后一行
		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for(int x = 1; x < xSize - 1; x++, vTop--, vMid++){
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
		

		return t;
	}

	/// <summary>
	///  创建底部的面
	/// </summary>
	private int CreateBottomFace(int[] triangles, int t, int ring){
		int v = 1;
		int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
		// 第一行
		t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
		for(int x = 1; x < xSize - 1; x++, v++, vMid++){
			t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

		// 中间行
		int vMin = ring - 2;
		vMid -= xSize - 2;
		int vMax = v + 2;
		for(int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++){
			t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
			for(int x = 1; x < xSize - 1; x++, vMid++){
				t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
			}
			t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
		}

		// 最后一行
		int vTop = vMin - 1;
		t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for(int x = 1; x < xSize - 1; x++, vTop--, vMid++){
			t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);
	 

		return t;

	}

	void OnDrawGizmos(){
		if(vertices == null) return ;

		Gizmos.color = Color.black;
		for(int i = 0; i < vertices.Length; i++){
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(vertices[i], 0.1f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(vertices[i], normals[i]);
		}
	}
}
