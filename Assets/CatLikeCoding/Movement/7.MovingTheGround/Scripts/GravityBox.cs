using UnityEngine;

namespace MovingTheGround
{
    public class GravityBox : GravitySource
    {
        [SerializeField]
        float gravity = 9.81f;

        [SerializeField]
        Vector3 boundaryDistance;

        [SerializeField]
        float innerDistance, innerFalloffDistance;

        [SerializeField]
        float outerDistance, outerFalloffDistance;

        float innerFalloffFactor;
        float outerFalloffFactor;

        void Awake()
        {
            OnValidate();
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            // 需要将计算后的向量转换到物体坐标系，因为这里只需要应用物体坐标的旋转，因此调用这个函数
            position = transform.InverseTransformDirection(position - transform.position);

            Vector3 vector = Vector3.zero;
            int outpoint = 0;
            if (position.x > boundaryDistance.x)
            {
                outpoint = 1;
                vector.x = boundaryDistance.x - position.x;
            }
            else if (position.x < -boundaryDistance.x) 
            {
                outpoint = 1;
                vector.x = -boundaryDistance.x - position.x;
            }

            if (position.y > boundaryDistance.y)
            {
                outpoint++;
                vector.y = boundaryDistance.y - position.y;
            }
            else if (position.y < -boundaryDistance.y)
            {
                outpoint++;
                vector.y = -boundaryDistance.y - position.y;
            }

            if (position.z > boundaryDistance.z)
            {
                outpoint++;
                vector.z = boundaryDistance.z - position.z;
            }
            else if (position.z < -boundaryDistance.z)
            {
                outpoint++;
                vector.z = -boundaryDistance.z - position.z;
            }

            if (outpoint > 0)
            {
                float distance = outpoint == 1 ? Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude;

                if (distance > outerFalloffDistance)
                {
                    return Vector3.zero;
                }

                float g = gravity / distance;
                if (distance > outerDistance)
                {
                    g *= 1 - (distance - outerDistance) * outerFalloffFactor;
                }

                return transform.TransformDirection(vector * g);
            }

            Vector3 distances;
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
            }
            else 
            {
                vector.z = GetGravityComponent(position.z, distances.z);
            }

            return vector;
        }

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

            return coordinate > 0 ? g : -g;
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Vector3 size = Vector3.zero;
            Gizmos.color = Color.cyan;
            if (innerFalloffDistance > innerDistance)
            {
                size.x = boundaryDistance.x - innerFalloffDistance;
                size.y = boundaryDistance.y - innerFalloffDistance;
                size.z = boundaryDistance.z - innerFalloffDistance;

                Gizmos.DrawWireCube(Vector3.zero, size * 2);
            }

            if (outerFalloffDistance > outerDistance)
            {
                DrawCube(outerFalloffDistance);
            }
            
            Gizmos.color = Color.yellow;
            if (innerDistance > 0)
            {
                size.x = boundaryDistance.x - innerDistance;
                size.y = boundaryDistance.y - innerDistance;
                size.z = boundaryDistance.z - innerDistance;

                Gizmos.DrawWireCube(Vector3.zero, size * 2);
            }

            if (outerDistance > 0)
            {
                DrawCube(outerDistance);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, boundaryDistance * 2);
        }

        void DrawCube(float distance)
        {
            Vector3 a,b,c,d;

            // 左右面 
            a.y = b.y = -boundaryDistance.y;
            b.z = c.z = -boundaryDistance.z;
            c.y = d.y = boundaryDistance.y;
            a.z = d.z = boundaryDistance.z;
            a.x = b.x = c.x = d.x = -boundaryDistance.x - distance;
            DrawRect(a, b, c, d);
            a.x = b.x = c.x = d.x = boundaryDistance.x + distance;
            DrawRect(a, b, c, d);

            // 前后面
            a.y = b.y = -boundaryDistance.y;
            b.x = c.x = -boundaryDistance.x;
            c.y = d.y = boundaryDistance.y;
            d.x = a.x = boundaryDistance.x;
            a.z = b.z = c.z = d.z = boundaryDistance.z + distance;
            DrawRect(a, b, c, d);
            a.z = b.z = c.z = d.z = -boundaryDistance.z - distance;
            DrawRect(a, b, c, d);

            // 上下面
            a.z = b.z = -boundaryDistance.z;
            b.x = c.x = boundaryDistance.x;
            c.z = d.z = boundaryDistance.z;
            d.x = a.x = -boundaryDistance.x;
            a.y = b.y = c.y = d.y = boundaryDistance.y + distance;
            DrawRect(a, b, c, d);
            a.y = b.y = c.y = d.y = -boundaryDistance.y - distance;
            DrawRect(a, b, c, d);

            distance *= 0.577350269f;
            Vector3 size = Vector3.zero;
            size.x = 2 * (boundaryDistance.x + distance);
            size.y = 2 * (boundaryDistance.y + distance);
            size.z = 2 * (boundaryDistance.z + distance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        void DrawRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }

        void OnValidate()
        {
            innerFalloffFactor = 1 / (innerFalloffDistance - innerDistance);
            outerFalloffFactor = 1 / (outerFalloffDistance - outerDistance);
        }
    }
}