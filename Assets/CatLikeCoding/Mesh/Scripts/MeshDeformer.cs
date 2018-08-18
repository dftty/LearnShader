using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;

	public float springForce = 20f;

	public float damping = 5f;

	// Use this for initialization
	void Start () {
		Debug.Log(new Vector3(0, 2, 0).sqrMagnitude);
		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		Array.Copy(originalVertices, displacedVertices, originalVertices.Length);
		vertexVelocities = new Vector3[originalVertices.Length];
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < displacedVertices.Length; i++){
			UpdateVertex(i);
		}

		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
	}

	public void UpdateVertex(int i){
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
		velocity -= displacement * springForce * Time.deltaTime;
		velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * Time.deltaTime;
	}

	public void AddDeformingForce(Vector3 point, float force){
		for(int i = 0; i < displacedVertices.Length; i++){
			AddForceToVertex(i, point, force);
		}
	}

	public void AddForceToVertex(int i , Vector3 point, float force){
		Vector3 pointToVertex = displacedVertices[i] - point;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}
}	
