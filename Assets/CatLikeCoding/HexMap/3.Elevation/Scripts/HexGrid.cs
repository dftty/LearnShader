using UnityEngine;
using UnityEngine.UI;

namespace Elevation
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

            for (int z = 0, i = 0; z < height; z++)
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
            Debug.Log("touched at: " + coordinates.ToString());
        }

        void CreateCell(int x, int z, int i)
        {
            Vector3 position;
            position.x = (x + z * 0.5f - z / 2) * HexMatrix.innerRadius * 2f;
            position.y = 0;
            position.z = z * HexMatrix.outerRadius * 1.5f;

            HexCell cell = Instantiate<HexCell>(cellPrefab);

            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;
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

            Text labelText = Instantiate<Text>(cellLabelPrefab);
            labelText.text = cell.coordinates.ToStringOnSeparateLines();

            labelText.transform.SetParent(gridCanvas.transform, false);
            labelText.rectTransform.anchoredPosition = new Vector2(position.x, position.z);

            cells[i] = cell;
        }
    }
}