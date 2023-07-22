using UnityEngine;

namespace Rolling
{
    public class GravitySphere : GravitySource
    {
        [SerializeField]
        float gravity =9.81f;

        [SerializeField]
        float outerRadius = 0, outerFalloffRadius = 0;

        [SerializeField]
        float innerRadius = 0, innerFalloffRadius = 0;

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
                g += 1 - (innerRadius - distance) * innerFalloffFactor;
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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, outerRadius);
            Gizmos.DrawWireSphere(transform.position, innerRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, outerFalloffRadius);
            Gizmos.DrawWireSphere(transform.position, innerFalloffRadius);
        }
    }
}