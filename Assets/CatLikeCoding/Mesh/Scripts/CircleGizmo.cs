using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleGizmo : MonoBehaviour {

	public int resolution = 10;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// 只显示被选中的物体的gizmo
	// 画出一个单位圆
	private void OnDrawGizmosSelected(){
		float step = 2f / resolution;
		for(int i = 0; i <= resolution; i++){
			ShowPoint(i * step - 1f, -1f);
			ShowPoint(i * step - 1f, 1f);
		}

		for(int i = 1; i < resolution; i++){
			ShowPoint(-1f, i * step - 1f);
			ShowPoint(1f, i * step - 1f);
		}
	}

	/**
	现在我们要映射square上的点到circle， 点是可以用向量来描述的， 所以我们的映射可以简单的用向量 Vs 的标准化来表示


	 */
	private void ShowPoint(float x, float y){
		// 首先画出矩形
		Vector2 square = new Vector2(x, y);
		// 圆的坐标就是矩形坐标的向量化
		Vector2 circle = square.normalized;

		//
		circle.x = square.x * Mathf.Sqrt(1f - square.y * square.y * 0.5f);
		circle.y = square.y * Mathf.Sqrt(1f - square.x * square.x * 0.5f);

		Gizmos.color = Color.black;
		Gizmos.DrawSphere(square, 0.025f);

		Gizmos.color = Color.white;
		Gizmos.DrawSphere(square, 0.025f);

		// 画出矩形到圆的线
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(square, circle);

		// 画出圆到原点的线
		Gizmos.color = Color.gray;
		Gizmos.DrawLine(circle, Vector2.zero);
	}
}
