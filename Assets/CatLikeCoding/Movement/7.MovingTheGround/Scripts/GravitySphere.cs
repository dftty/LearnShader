using UnityEngine;

namespace MovingTheGround
{
    public class GravitySphere : GravitySource
    {
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField, Min(0)]
        float outerRadis = 10, outerFalloffRadis = 15f;
        
        [SerializeField]
        float innerRadis = 0, innerFalloffRadis = 5;

        float outerFalloffFactor;
        float innerFalloffFactor;

        void Awake()
        {
            OnValidate();
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            Vector3 vector = transform.position - position;
            float distance = vector.magnitude;

            if (distance > outerFalloffRadis || distance < innerFalloffRadis)
            {
                return Vector3.zero;
            }

            // 因为这里已经除以distance了，因此最后vector返回无需归一化
            float g = gravity / distance;
            if (distance > outerRadis)
            {
                g *= 1 - (distance - outerRadis) * outerFalloffFactor;
            }
            else if (distance < innerRadis)
            {
                g *= 1 - (innerRadis - distance) * innerFalloffFactor;
            }
            
            return vector * g;
        }

        void OnValidate()
        {
            innerFalloffRadis = Mathf.Max(innerFalloffRadis, 0);
            innerRadis = Mathf.Max(innerRadis, innerFalloffRadis);

            outerRadis = Mathf.Max(outerRadis, innerRadis);
            outerFalloffRadis = Mathf.Max(outerFalloffRadis, outerRadis);

            outerFalloffFactor = 1 / (outerFalloffRadis - outerRadis);
            innerFalloffFactor = 1 / (innerRadis - innerFalloffFactor);
        }

        void OnDrawGizmos()
        {
            Vector3 center = transform.position;

            if (outerRadis > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(center, outerRadis);
            }

            if (outerFalloffRadis > outerRadis)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, outerFalloffRadis);
            }

            if (innerRadis > innerFalloffRadis)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(center, innerRadis);
            }

            if (innerFalloffRadis > 0)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(center, innerFalloffRadis);
            }

        }
    }
}