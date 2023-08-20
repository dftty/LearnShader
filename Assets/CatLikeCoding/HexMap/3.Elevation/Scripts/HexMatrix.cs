using UnityEngine;

namespace Elevation
{
    public static class HexMatrix
    {
        public const float outerRadius = 10f;

        public const float innerRadius = outerRadius * 0.866025404f;

        public const float solidFactor = 0.75f;

        public const float blendFactor = 1f - solidFactor;

        readonly static Vector3[] cornors = 
        {
            new Vector3(0, 0, outerRadius),
            new Vector3(innerRadius, 0, 0.5f * outerRadius),
            new Vector3(innerRadius, 0, -0.5f * outerRadius),
            new Vector3(0, 0, -outerRadius),
            new Vector3(-innerRadius, 0, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0, 0.5f * outerRadius),
            new Vector3(0, 0, outerRadius)
        };

        public static Vector3 GetFirstCorner(HexDirection direction)
        {
            return cornors[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction)
        {
            return cornors[(int)direction + 1];
        }

        public static Vector3 GetFirstSolidCorner(HexDirection direction)
        {
            return cornors[(int)direction] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection direction)
        {
            return cornors[(int)direction + 1] * solidFactor;
        }

        public static Vector3 GetBridge(HexDirection direction)
        {
            return blendFactor * (cornors[(int)direction] + cornors[(int)direction + 1]);
        }
    }
}