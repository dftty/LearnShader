using UnityEngine;

namespace SurfaceContace
{
    public class MovingSphere : MonoBehaviour
    {

        [SerializeField, Range(1f, 100f)]
        float maxSpeed = 10f;

        [SerializeField, Range(1f, 100f)]
        float maxAccerlation = 10f, maxAirAccerlation = 1;

        [SerializeField, Range(0f, 90f)]
        float maxGroundAngle = 25f;

        [SerializeField, Range(1, 5)]
        int maxAirJump = 1;

        [SerializeField, Range(1f, 5f)]
        float jumpHeight = 2;

        Rigidbody body;

        Vector3 velocity = Vector3.zero;
        Vector3 desireVelocity = Vector3.zero;
        Vector3 contactNormal;
        float maxGroundDotProduct;
        float groundContactCount = 0;
        bool desireJump = false;
        int jumpPhase;
        bool onGround => groundContactCount > 0;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            maxGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
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

            GetComponent<Renderer>().material.SetColor("_Color", Color.white * (groundContactCount * 0.25f));
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
            if (onGround || (!onGround) && jumpPhase < maxAirJump)
            {
                jumpPhase += 1;
                // 根据跳跃高度计算起跳速度，因为物理常量重力是负值，因此用-2f
                float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
                float alignSpeed = Vector3.Dot(velocity, contactNormal);

                // 为了防止多次连跳跳跃速度越来越大，每次跳跃时计算是否需要用起跳速度减去当前物体的上升速度
                if (alignSpeed > 0)
                {
                    jumpSpeed = Mathf.Max(jumpSpeed - alignSpeed, 0);
                }

                velocity += contactNormal * jumpSpeed;
            }
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
            // 首先，从刚体上获取物体的当前速度
            velocity = body.velocity;

            if (onGround)
            {
                jumpPhase = 0;
                contactNormal.Normalize();
            }
            else 
            {
                contactNormal = Vector3.up;
            }
        }

        void ClearState() 
        {
            groundContactCount = 0;
            contactNormal = Vector3.zero;
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
            for (int i = 0; i < other.contactCount; i++)
            {
                Vector3 normal = other.GetContact(i).normal;

                if (normal.y > maxGroundDotProduct)
                {
                    groundContactCount += 1;
                    contactNormal += normal;
                }
            }
        }
    }
}