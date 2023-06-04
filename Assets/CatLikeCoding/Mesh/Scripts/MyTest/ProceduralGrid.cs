using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGrid : MonoBehaviour
{

    public int xSize, ySize;

    private Vector3[] vertices;

    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Generate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Generate()
    {
        yield return null;
        // 顶点数组数量
        vertices = new Vector3[(xSize + 1) * (ySize + 1)];

        // 生成顶点
        // 多层遍历时，可以在外层定义一个变量，用于记录当前遍历的层数
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
                yield return new WaitForSeconds(0.1f);
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices;

        int[] triangles = new int[xSize * ySize * 6];

        // 生成三角形
        // 三角形的顶点顺序决定了三角形的正反面
        // 顺时针为正面，逆时针为反面 顺时针在Unity内是Z轴正面可见，Z轴正面就是你看向绘制出来的这个面的方向
        for (int i = 0, y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++, i += 6)
            {
                triangles[i] = x + y * (xSize + 1);
                triangles[i + 1] = triangles[i + 4] = x + (y + 1) * (xSize + 1);
                triangles[i + 2] = triangles[i + 3] = x + 1 + y * (xSize + 1);
                triangles[i + 5] = x + 1 + (y + 1) * (xSize + 1);
            }
        }

        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
