using UnityEngine;

namespace ComplexGravity 
{
    public class GravityBox : GravitySource
    {
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField]
        Vector3 boundaryDistance = Vector3.one;

        [SerializeField, Min(0f)]
        float innerDistance = 0f, innerFalloffDistance = 0f;

        [SerializeField, Min(0f)]
        float outerDistance = 0f, outerFalloffDistance = 0f;

        float innerFalloffFactor;
        float outerFalloffFactor;

        void Awake()
        {

        }

        /// <summary>
        /// 此类型仅会存在一个分量上有重力
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override Vector3 GetGravity(Vector3 position)
        {
            position = transform.InverseTransformDirection(position - transform.position);
            Vector3 vector = Vector3.zero;

            int outside = 0;
            if (position.x > boundaryDistance.x)
            {
                outside = 1;
                vector.x = boundaryDistance.x - position.x;
            }
            else if (position.x < -boundaryDistance.x)
            {
                outside = 1;
                vector.x = -boundaryDistance.x - position.x;
            }

            if (position.y > boundaryDistance.y)
            {
                outside += 1;
                vector.y = boundaryDistance.y - position.y;
            }
            else if (position.y < -boundaryDistance.y)
            {
                outside += 1;
                vector.y = -boundaryDistance.y - position.y;
            }

            if (position.z > boundaryDistance.z)
            {
                outside += 1;
                vector.z = boundaryDistance.z - position.z;
            }
            else if (position.z < -boundaryDistance.z)
            {
                outside += 1;
                vector.z = -boundaryDistance.z - position.z;
            }

            if (outside > 0)
            {
                // 当outside为1时，说明vector中仅有一个分量有值
                float distance = outside == 1 ? Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude;
                if (distance > outerFalloffDistance)
                {
                    return Vector3.zero;
                }
                float g = gravity / distance;
                if (distance > outerDistance)
                {
                    g *= 1 - (distance - outerDistance) * outerFalloffFactor;
                }

                return transform.TransformDirection(g * vector);
            }

            // distance表示从物体指向盒子中心的向量
            Vector3 distances = Vector3.zero;
            // 当物体在box的上下平面上时，此时y值为0，x和y值在0-10之间
            distances.x = boundaryDistance.x - Mathf.Abs(position.x);
            distances.y = boundaryDistance.y - Mathf.Abs(position.y);
            distances.z = boundaryDistance.z - Mathf.Abs(position.z);
            if (distances.x < distances.y)
            {
                if (distances.x < distances.z)
                {
                    vector.x = GetGravityComponent(position.x, distances.x);
                }
                else 
                {
                    vector.z = GetGravityComponent(position.z, distances.z);
                }
            }
            else if (distances.y < distances.z)
            {
                vector.y = GetGravityComponent(position.y, distances.y);

                Debug.Log(vector.y);
            }
            else 
            {
                vector.z = GetGravityComponent(position.z, distances.z);
            }
            return vector;
        }

        /// <summary>
        /// 该函数仅用于计算内部重力
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        float GetGravityComponent(float coordinate, float distance)
        {
            if (distance > innerFalloffDistance)
            {
                return 0;
            }

            float g = gravity;
            if (distance > innerDistance)
            {
                g *= 1 - (distance - innerDistance) * innerFalloffFactor;
            }

            return coordinate > 0 ? -g : g;
        }
        
        void OnValidate()
        {
            boundaryDistance = Vector3.Max(boundaryDistance, Vector3.zero);
            float maxInner = Mathf.Min(Mathf.Min(boundaryDistance.x, boundaryDistance.y), boundaryDistance.z);
            innerDistance = Mathf.Min(maxInner, innerDistance);
            innerFalloffDistance = Mathf.Max(Mathf.Min(innerFalloffDistance, maxInner), innerDistance);



            innerFalloffFactor = 1 / (innerFalloffDistance - innerDistance);
            outerFalloffFactor = 1 / (outerFalloffDistance - outerDistance);
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Vector3 size = Vector3.zero;
            if (innerFalloffDistance > innerDistance)
            {
                size.x = boundaryDistance.x - innerFalloffDistance;
                size.y = boundaryDistance.y - innerFalloffDistance;
                size.z = boundaryDistance.z - innerFalloffDistance;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.zero, 2 * size);
            }

            if (outerFalloffDistance > outerDistance)
            {
                Gizmos.color = Color.cyan;
                DrawGizmoOuterCube(outerFalloffDistance);
            }

            if (innerDistance > 0)
            {
                size.x = boundaryDistance.x - innerDistance;
                size.y = boundaryDistance.y - innerDistance;
                size.z = boundaryDistance.z - innerDistance;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, 2 * size);
            }

            if (outerDistance > 0)
            {
                Gizmos.color = Color.yellow;
                DrawGizmoOuterCube(outerDistance);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, 2 * boundaryDistance);
        }

        void DrawGizmoOuterCube(float distance)
        {
            Vector3 a,b,c,d;

            // 绘制左右面
            a.y = d.y = -boundaryDistance.y;
            b.y = c.y = boundaryDistance.y;
            a.z = b.z = boundaryDistance.z;
            c.z = d.z = -boundaryDistance.z;
            a.x = b.x = c.x = d.x = -boundaryDistance.x - distance;
            DrawGizmoRect(a, b, c, d);
            a.x = b.x = c.x = d.x = boundaryDistance.x + distance;
            DrawGizmoRect(a, b, c, d);

            // 前后面
            a.x = b.x = -boundaryDistance.x;
            a.y = d.y = -boundaryDistance.y;
            b.y = c.y = boundaryDistance.y;
            c.x = d.x = boundaryDistance.x;
            a.z = b.z = c.z = d.z = -boundaryDistance.z - distance;
            DrawGizmoRect(a, b, c, d);
            a.z = b.z = c.z = d.z = boundaryDistance.z + distance;
            DrawGizmoRect(a, b, c, d);

            // 上下面
            a.x = b.x = -boundaryDistance.x;
            a.z = d.z = -boundaryDistance.z;
            c.x = d.x = boundaryDistance.x;
            c.z = b.z = boundaryDistance.z;
            a.y = b.y = c.y = d.y = -boundaryDistance.y - distance;
            DrawGizmoRect(a, b, c, d);
            a.y = b.y = c.y = d.y = boundaryDistance.y + distance;
            DrawGizmoRect(a, b, c, d);

            // 外部box理论上应该是一个圆角的box，这里为了简化，绘制圆角点处的box
            // 圆角的distance为正常distance的√(1 / 3)
            distance *= 0.5773502692f;
            Vector3 size = boundaryDistance;
            size.x = 2 * (size.x + distance);
            size.y = 2 * (size.y + distance);
            size.z = 2 * (size.z + distance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        void DrawGizmoRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }
}