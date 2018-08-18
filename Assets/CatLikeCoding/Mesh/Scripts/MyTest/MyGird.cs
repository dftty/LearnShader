using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MyGird : MonoBehaviour {

	public int girdSize;

	private Vector3[] vertices;

	void Awake(){
		StartCoroutine(Generate());
	}

	IEnumerator Generate(){
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		vertices = new Vector3[girdSize * girdSize];

		// 创建点
		for(int i = 0, index = 0; i < girdSize; i++){
			for(int j = 0; j < girdSize; j++, index++){
				vertices[index] = new Vector3(j, 0, i);
				mesh.vertices = vertices;
				yield return new WaitForSeconds(0.1f);
			}
		}

		// 创建面
		int[] triangles = new int[(girdSize - 1) * (girdSize - 1) * 6];
		for(int i = 0, index = 0; i < girdSize - 1; i++){
			for(int j = 0; j < girdSize - 1; j++, index += 6){
				triangles[index] = i * girdSize + j;
				triangles[index + 1] = triangles[index + 4] = i * girdSize + j + 1;
				triangles[index + 2] = triangles[index + 3] = (i + 1) *girdSize + j;
				triangles[index + 5] = (i + 1) *girdSize + j + 1;
			}
		}

		mesh.triangles = triangles;

		yield return null;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnDrawGizmos() {
		if(vertices != null){
			for(int i = 0; i < vertices.Length; i++){
				if(vertices[i] != null){
					Gizmos.color = Color.black;
					Gizmos.DrawSphere(vertices[i], 0.1f);
				}
			}
		}
	}
}
