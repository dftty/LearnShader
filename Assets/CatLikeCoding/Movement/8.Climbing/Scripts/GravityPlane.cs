using UnityEngine;

namespace Climbing
{
    public class GravityPlane : GravitySource
    {   
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField]
        float range = 4f;

        public override Vector3 GetGravity(Vector3 position)
        {
            Vector3 up = transform.up;
            float distance = Vector3.Dot(up, position - transform.position);
            if (distance > range)
            {
                return Vector3.zero;
            }

            float g = -gravity;
            if (distance > 0)
            {
                g *= 1 - distance / range;
            }

            return up * g;
        }

        void OnDrawGizmos()
        {
            Vector3 scale = transform.localScale;
            scale.y = range;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);

            Vector3 size = new Vector3(1, 0, 1);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, size);

            if (range > 0)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.up, size);
            }
        }
    }
}