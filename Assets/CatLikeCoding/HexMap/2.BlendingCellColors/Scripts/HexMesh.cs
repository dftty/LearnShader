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
            Vector3 pos = cell.transform.position;

            for (int i = 0; i < 6; i++)
            {
                AddTriangle(
                    pos, 
                    pos + HexMatrix.corners[i], 
                    pos + HexMatrix.corners[ i + 1]);
                AddTriangleColor(cell.color);
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

        void AddTriangleColor(Color color)
        {
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
        }
    }   
}