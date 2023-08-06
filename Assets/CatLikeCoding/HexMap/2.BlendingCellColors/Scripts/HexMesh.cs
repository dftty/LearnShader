using UnityEngine;
using System.Collections.Generic;

namespace BlendingCellColors
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {

        Mesh hexMesh;

        List<Vector3> vertices;

        List<int> triangles;

        MeshCollider meshCollider;

        List<Color> colors;

        void Awake()
        {
            GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
            meshCollider = GetComponent<MeshCollider>();
            vertices = new List<Vector3>();
            triangles = new List<int>();
            colors = new List<Color>();
        }

        public void Triangulate(HexCell[] cells)
        {
            hexMesh.Clear();
            vertices.Clear();
            triangles.Clear();
            colors.Clear();

            for (int i = 0; i < cells.Length; i++)
            {
                Triangulate(cells[i]);
            }

            hexMesh.vertices = vertices.ToArray();
            hexMesh.triangles = triangles.ToArray();
            hexMesh.colors = colors.ToArray();
            hexMesh.RecalculateNormals();

            meshCollider.sharedMesh = hexMesh;
        }

        void Triangulate(HexCell cell)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(cell, d);       
            }
        }

        void Triangulate(HexCell cell, HexDirection d)
        {
            Vector3 pos = cell.transform.position;

            Vector3 v1 = pos + HexMatrix.GetFirstSolodCorner(d);
            Vector3 v2 = pos + HexMatrix.GetSecondSolidCorner(d);

            AddTriangle(pos, v1, v2);
            AddTriangleColor(cell.color, cell.color, cell.color);

            // 连接面仅需要填充三个方向上的
            if (d <= HexDirection.SE)
            {
                TriangulateConnection(cell, d, v1, v2);
            }
        }

        void TriangulateConnection(HexCell cell, HexDirection d, Vector3 v1, Vector3 v2)
        {
            HexCell neighbor = cell.GetNeighbor(d);
            if (neighbor == null)
            {
                return ;
            }

            Vector3 bridge = HexMatrix.GetBridge(d);
            Vector3 v3 = v1 + bridge;
            Vector3 v4 = v2 + bridge;

            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.color, neighbor.color);

            HexCell nextNeighbor = cell.GetNeighbor(d.Next()) ;

            // 三角仅需要填充两个方向上的
            if (d <= HexDirection.E && nextNeighbor != null)
            {
                AddTriangle(v2, v4, v2 + HexMatrix.GetBridge(d.Next()));
                AddTriangleColor(
                    cell.color,
                    neighbor.color,
                    nextNeighbor.color
                );
            }
        }

        void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int index = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }

        void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);
        }

        void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            int index = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        void AddQuadColor(Color c1, Color c2)
        {
            colors.Add(c1);
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c2);
        }
    }   
}