using UnityEngine;
using System.Collections.Generic;

namespace Elevation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        Mesh mesh;

        MeshCollider meshCollider;

        List<Vector3> vertices;

        List<int> triangles;

        List<Color> colors;

        void Awake()
        {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            meshCollider = GetComponent<MeshCollider>();
            vertices = new List<Vector3>();
            triangles = new List<int>();
            colors = new List<Color>();
        }

        public void Triangulate(HexCell[] cells)
        {
            mesh.Clear();
            vertices.Clear();
            triangles.Clear();
            colors.Clear();

            for (int i = 0; i < cells.Length; i++)
            {
                Triangulate(cells[i]);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors.ToArray();
            mesh.RecalculateNormals();

            meshCollider.sharedMesh = mesh;
        }

        void Triangulate(HexCell cell)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, cell);
            }
        }

        void Triangulate(HexDirection direction, HexCell cell)
        {
            Vector3 position = cell.transform.position;

            Vector3 v1 = position + HexMatrix.GetFirstSolidCorner(direction);
            Vector3 v2 = position + HexMatrix.GetSecondSolidCorner(direction);

            AddTriangle(position, v1, v2);
            AddTriangleColor(cell.color, cell.color, cell.color);

            if (direction <= HexDirection.SE)
            {
                TriangulateConnection(direction, cell, v1, v2);
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

        void TriangulateConnection(HexDirection d, HexCell cell, Vector3 v1, Vector3 v2)
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

            HexCell nextNeighbor = cell.GetNeighbor(d.Next());

            if (d <= HexDirection.E && nextNeighbor != null)
            {
                // v2 加上下一个桥的位置
                Vector3 v5 = v2 + HexMatrix.GetBridge(d.Next());
                AddTriangle(v2, v4, v5);
                AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
            }
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