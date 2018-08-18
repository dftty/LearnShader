using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTransformation : Transformation {

	public Vector3 rotation;

	public override Matrix4x4 Matrix{
		get{
			// 先将角度转换为弧度
			float radX = rotation.x * Mathf.Deg2Rad;
			float radY = rotation.y * Mathf.Deg2Rad;
			float radZ = rotation.z * Mathf.Deg2Rad;
			// sin 和cos 函数接受弧度参数　 
			float sinX = Mathf.Sin(radX);
			float cosX = Mathf.Cos(radX);
			float sinY = Mathf.Sin(radY);
			float cosY = Mathf.Cos(radY);
			float sinZ = Mathf.Sin(radZ);
			float cosZ = Mathf.Cos(radZ);


			Matrix4x4 matrix = new Matrix4x4();
			matrix.SetColumn(0, new Vector3(
				cosY * cosZ,
				cosX * sinZ + sinX * sinY * cosZ,
				sinX * sinZ - cosX * sinY * cosZ
			)); 

			matrix.SetColumn(1, new Vector3(
				-cosY * sinZ,
				cosX * cosZ - sinX * sinY *sinZ,
				sinX * cosZ + cosX * sinY * sinZ
			));

			matrix.SetColumn(2, new Vector3(
				sinY,
				-sinX * cosY,
				cosX * cosY
			));
			matrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

			return matrix; 
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
