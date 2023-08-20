using UnityEngine;
using UnityEngine.EventSystems;

namespace Elevation
{
    public class HexMapEditor : MonoBehaviour
    {
        public Color[] colors;

        public HexGrid hexGrid;

        Color selectedColor;

        void Update()
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                HandleInput();
            }
            
        }

        void HandleInput()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                hexGrid.ColorCell(hit.point, selectedColor);
            }
        }

        public void SelectColor(int index)
        {
            selectedColor = colors[index];
        }
    }
}