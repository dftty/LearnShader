using UnityEngine;

namespace ComplexGravity
{
    public class GravitySphere : GravitySource
    {   
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField, Min(0)]
        float innerRadius = 0, innerFalloffRadius = 5f;

        [SerializeField, Min(0)]
        float outerRadius = 10, outerFalloffRadius = 15f;


        float falloffFactor;
        float innerFalloffFactor;

        void Awake()
        {
            OnValidate();
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            Vector3 vector = transform.position - position;
            float distance = vector.magnitude;

            if (distance > outerFalloffRadius || distance < innerFalloffRadius)
            {
                return Vector3.zero;
            }

            float g = gravity / distance;
            if (distance > outerRadius)
            {
                g *= 1 - (distance - outerRadius) * falloffFactor;
            }
            else if (distance < innerRadius)
            {
                g *= 1 - (innerRadius - distance) * innerFalloffFactor;
            }

            return g * vector;
        }

        void OnDrawGizmos()
        {
            Vector3 center = transform.position;
            if (innerFalloffRadius > 0 && innerRadius > innerFalloffRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, innerFalloffRadius);
            }

            Gizmos.color = Color.yellow;
            if (innerRadius > 0 && innerRadius < outerRadius)
            {
                Gizmos.DrawWireSphere(center, innerRadius);
            }
            Gizmos.DrawWireSphere(center, outerRadius);

            if (outerFalloffRadius > outerRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, outerFalloffRadius);
            }
        }

        void OnValidate()
        {
            innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0f);
            innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);
            outerRadius = Mathf.Max(outerRadius, innerRadius);
            outerFalloffRadius = Mathf.Max(outerRadius, outerFalloffRadius);

            falloffFactor = 1 / (outerFalloffRadius - outerRadius);
            innerFalloffFactor = 1 / (innerRadius - innerFalloffFactor);
        }
    }
}