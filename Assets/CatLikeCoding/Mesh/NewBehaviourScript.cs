using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

	public Mesh mesh;

	// Use this for initialization
	void Start () {
		mesh = GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDrawGizmos()
	{
		if (mesh == null) return;
		Gizmos.color = Color.black;
		//for (int i = 0; i < mesh.vertices.Length; i++)
		//{
		int i = 0;
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(mesh.vertices[i], mesh.normals[i]);

			Gizmos.color = Color.red;
			Gizmos.DrawRay(mesh.vertices[i], mesh.tangents[i]);
			

			Vector3 v3 = Vector3.Cross(mesh.normals[i], mesh.tangents[i]);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(mesh.vertices[i], v3);
		//}
	}
}
