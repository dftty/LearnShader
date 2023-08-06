using UnityEngine;

namespace BlendingCellColors
{
    public static class HexMatrix
    {
        public const float outerRadius = 10;

        public const float innerRadius = outerRadius * 0.866025404f;

        public const float solidFactor = 0.75f;

        public const float blendFactor = 1 - solidFactor;

        public static Vector3[] corners = 
        {
            new Vector3(0, 0, outerRadius),
            new Vector3(innerRadius, 0, 0.5f * outerRadius),
            new Vector3(innerRadius, 0, -0.5f * outerRadius),
            new Vector3(0, 0, -outerRadius),
            new Vector3(-innerRadius, 0, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0, 0.5f * outerRadius),
            new Vector3(0, 0, outerRadius)
        };

        public static Vector3 GetBridge(HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
        }

        public static Vector3 GetFirstCorner(HexDirection direction)
        {
            return corners[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction)
        {
            return corners[(int)direction + 1];
        }

        public static Vector3 GetFirstSolodCorner(HexDirection direction)
        {
            return corners[(int)direction] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection direction)
        {
            return corners[(int)direction + 1] * solidFactor;
        }
    }   
}