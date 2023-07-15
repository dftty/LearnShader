using UnityEngine;

namespace Swimming
{
    public class GravitySphere : GravitySource
    {   
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField, Min(0)]
        float outerRadius, outerFalloffRadius;

        [SerializeField]
        float innerRadius, innerFalloffRadius;

        float outerFalloffFactor;
        float innerFalloffFactor;

        void Awake()
        {
            OnValidate();
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            position -= transform.position;
            float distance = position.magnitude;

            if (distance > outerFalloffRadius || distance < innerFalloffRadius)
            {
                return Vector3.zero;
            }

            float g = -gravity / distance;
            if (distance > outerRadius)
            {
                g *= 1 - (distance - outerRadius) * outerFalloffFactor;
            }
            else if (distance < innerRadius)
            {
                g *= 1 - (innerRadius - distance) * innerFalloffFactor;
            }

            return position * g;
        }

        void OnValidate()
        {
            outerFalloffFactor = 1 / (outerFalloffRadius - outerRadius);
            innerFalloffFactor = 1 / (innerRadius - innerFalloffRadius);
        }

        void OnDrawGizmos()
        {
            Vector3 center = transform.position;

            if (outerFalloffRadius > outerRadius)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(center, outerFalloffRadius);
            }

            if (innerFalloffRadius > 0)
            {
                Gizmos.DrawWireSphere(center, innerFalloffRadius);
            }

            if (outerRadius > 0)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, outerRadius);
            }

            if (innerRadius > innerFalloffRadius)
            {
                Gizmos.DrawWireSphere(center, innerRadius);
            }
        }
    }
}