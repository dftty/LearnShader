using UnityEngine;
using UnityEngine.UI;

namespace BlendingCellColors
{
    public class HexGrid : MonoBehaviour
    {
        public int width;

        public int height;

        public HexCell cellPrefab;

        public Text cellLabelPrefab;

        HexCell[] cells;

        Canvas gridCanvas;

        HexMesh hexMesh;

        void Awake()
        {
            gridCanvas = GetComponentInChildren<Canvas>();
            hexMesh = GetComponentInChildren<HexMesh>();
            cells = new HexCell[width * height];

            for (int z = 0, i = 0; z < height ; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        void Start()
        {
            hexMesh.Triangulate(cells);
        }

        public void ColorCell(Vector3 position, Color color)
        {
            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
            HexCell cell = cells[index];
            cell.color = color;
            hexMesh.Triangulate(cells);
        }

        void CreateCell(int x, int z, int i)
        {
            Vector3 pos;
            pos.x = (x + 0.5f * z - z / 2) * HexMatrix.innerRadius * 2;
            pos.y = 0;
            pos.z = z * HexMatrix.outerRadius * 1.5f;

            HexCell cell = Instantiate<HexCell>(cellPrefab);
            cell.transform.SetParent(transform, false);
            cell.transform.position = pos;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, cells[i - 1]);
            }

            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                    }
                }
                else 
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                    if (x < width - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                    }                    
                }
            }

            Text label = Instantiate<Text>(cellLabelPrefab);
            label.rectTransform.SetParent(gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();

            cells[i] = cell;
        }
    }   
}