using UnityEngine;

namespace ComplexGravity
{
    public class GravityPlane : GravitySource
    {  
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField, Min(0f)]
        float range = 1;

        public override Vector3 GetGravity(Vector3 position)
        {
            Vector3 up = transform.up;
            // 计算玩家相对位置投影到物体up轴上的长度
            float distance = Vector3.Dot(up, position - transform.position);
            if (distance > range)
            {
                return Vector3.zero;
            }

            float g = -gravity;
            // 实现距离越远，重力越小
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
            Vector3 size = new Vector3(1f, 0, 1f);
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