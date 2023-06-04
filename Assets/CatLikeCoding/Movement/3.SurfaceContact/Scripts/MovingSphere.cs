using UnityEngine;

namespace SurfaceContace
{
    public class MovingSphere : MonoBehaviour
    {

        [SerializeField, Range(1f, 100f)]
        float maxSpeed = 30f;

        [SerializeField, Range(1f, 100f)]
        float maxAccerlation = 10f, maxAirAccerlation = 1;

        [SerializeField, Range(0f, 90f)]
        float maxGroundAngle = 25f, maxStairAngle = 50f;

        [SerializeField, Range(0, 5)]
        int maxAirJump = 1;

        [SerializeField, Range(1f, 5f)]
        float jumpHeight = 2;

        [SerializeField, Range(1f, 100f)]
        float maxSnapSpeed = 10;

        // 地面射线检测最大距离
        [SerializeField, Range(0f, 1f)]
        float probeDistance = 1;
        
        // 地面射线检测层级
        [SerializeField]
        LayerMask probeMask, stairMask;

        Rigidbody body;

        Vector3 velocity = Vector3.zero;
        Vector3 desireVelocity = Vector3.zero;
        Vector3 contactNormal, steepContactNormal;
        float groundContactCount, steepContactCount;
        bool onGround => groundContactCount > 0;
        bool onSteep => steepContactCount > 0;

        float minGroundDotProduct, minStairDotProduct;
        bool desireJump = false;
        int jumpPhase;
        int stepSinceLastGrounded, stepSinceLastJump;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
        }

        void Update() 
        {
            // 在Update中获取用户输入并计算对应的最大速度
            Vector2 playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            desireJump |= Input.GetButtonDown("Jump");

            // 对于使用pc键盘输入的用户，如果同时按下了水平和垂直输入，会导致x和y的值都为1，长度就是根号2，
            // 但是我们需要保证使用手柄和键盘输入的值，长度最大应该保持1，因此这里Clamp下
            playerInput = Vector2.ClampMagnitude(playerInput, 1);

            desireVelocity = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;

            GetComponent<Renderer>().material.SetColor("_Color", onGround ? Color.black : Color.white);
        }

        void FixedUpdate() 
        {
            UpdateState();
            AdjustVelocity();

            // 判断是否需要跳跃
            if (desireJump)
            {
                desireJump = false;
                Jump();
            }
            
            body.velocity = velocity;
            ClearState();
        }

        void Jump()
        {
            Vector3 jumpDirection;

            if (onGround)
            {
                jumpDirection = contactNormal;
            }
            else if (onSteep)
            {
                jumpDirection = steepContactNormal;
                // 墙跳也应该重置跳跃次数
                jumpPhase = 0;
            }
            else if (maxAirJump > 0 && jumpPhase <= maxAirJump)
            {
                // 这个判断是为了防止当空中跳跃次数大于0时，没有跳跃但是从平面掉落时，可以多跳一次的bug
                if (jumpPhase == 0)
                {
                    jumpPhase = 1;
                }
                jumpDirection = contactNormal;
            }
            else 
            {
                return ;
            }

            jumpDirection = (jumpDirection + Vector3.up).normalized;

            stepSinceLastJump = 0;
            jumpPhase += 1;
            // 根据跳跃高度计算起跳速度，因为物理常量重力是负值，因此用-2f
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignSpeed = Vector3.Dot(velocity, jumpDirection);

            // 为了防止多次连跳跳跃速度越来越大，每次跳跃时计算是否需要用起跳速度减去当前物体的上升速度
            if (alignSpeed > 0)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignSpeed, 0);
            }

            velocity += jumpDirection * jumpSpeed;
        }

        void AdjustVelocity()
        {
            // 根据加速度计算两个方向的速度增量
            float accerlation = onGround ? maxAccerlation : maxAirAccerlation;
            float maxSpeedChange = accerlation * Time.deltaTime;

            Vector3 xAxis = ProjectOnPlane(Vector3.right);
            Vector3 zAxis = ProjectOnPlane(Vector3.forward);

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            float newX = Mathf.MoveTowards(currentX, desireVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desireVelocity.z, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        Vector3 ProjectOnPlane(Vector3 vector)
        {
            return vector - contactNormal * Vector3.Dot(contactNormal, vector);
        }

        void UpdateState()
        {
            stepSinceLastGrounded += 1;
            stepSinceLastJump += 1;
            // 首先，从刚体上获取物体的当前速度
            velocity = body.velocity;

            if (onGround || SnapToGround() || CheckSteepContact())
            {
                stepSinceLastGrounded = 0;
                // 因为在空中第一帧我们用SnapToGround判定为还在地面，所以这里需要判断跳跃step
                if (stepSinceLastJump > 1)
                {
                    jumpPhase = 0;
                }
                contactNormal.Normalize();
            }
            else 
            {
                contactNormal = Vector3.up;
            }
        }

        bool SnapToGround()
        {
            if (stepSinceLastGrounded > 1 || stepSinceLastJump <= 2)
            {
                return false;
            }

            // 当速度大于最大贴地速度时，物体应该表现和现实一样，飞起来
            float speed = velocity.magnitude;
            if (speed > maxSnapSpeed)
            {
                return false;
            }

            // 没有检测到碰撞体
            if (!Physics.Raycast(transform.position, Vector3.down, out var hit, probeDistance, probeMask))
            {
                return false;
            }

            // 检测到的碰撞体不是地面
            if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
            {
                return false;
            }

            groundContactCount = 1;
            contactNormal = hit.normal;

            float dot = Vector3.Dot(velocity, contactNormal);

            if (dot > 0)
            {
                velocity = (velocity - contactNormal * dot).normalized * speed;
            }

            return false;
        }

        /// <summary>
        /// 该函数用于当场景中出现Crevasse地形时，处理玩家可能无法跳跃的情况
        /// </summary>
        /// <returns></returns>
        bool CheckSteepContact()
        {
            // 当同时检测到多个陡坡
            if (steepContactCount > 1)
            {
                steepContactNormal.Normalize();

                // 如果该向量满足地面条件，那么就可以把该向量当做虚拟的当前地面向量
                if (steepContactNormal.y > minGroundDotProduct)
                {
                    groundContactCount = 1;
                    contactNormal = steepContactNormal;
                    return true;
                }
            }
            return false;
        }

        void ClearState() 
        {
            groundContactCount = 0;
            contactNormal = Vector3.zero;

            steepContactCount = 0;
            steepContactNormal = Vector3.zero;
        }

        void OnCollisionEnter(Collision other) 
        {
            EvaluateCollision(other);
        }

        void OnCollisionStay(Collision other) 
        {
            EvaluateCollision(other);
        }

        void EvaluateCollision(Collision other)
        {
            float dot = GetMinDot(other.gameObject.layer);
            for (int i = 0; i < other.contactCount; i++)
            {
                Vector3 normal = other.GetContact(i).normal;

                if (normal.y > dot)
                {
                    groundContactCount += 1;
                    contactNormal += normal;
                }
                else if (normal.y > -0.01f)
                {
                    steepContactCount += 1;
                    steepContactNormal += normal;
                }
            }
        }

        // 检测到斜坡时，如果层级是楼梯，那么应该选取minStairDotProduct
        float GetMinDot(int layer)
        {
            return ((stairMask & (1 << layer)) == 0) ? minGroundDotProduct : minStairDotProduct;
        }
    }
}