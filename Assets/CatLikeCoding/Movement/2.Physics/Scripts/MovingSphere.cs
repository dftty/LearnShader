using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementPhysics
{
    public class MovingSphere : MonoBehaviour
    {

        [SerializeField, Range(10f, 100f)]
        float maxAirAccerlation = 5;

        [SerializeField, Range(10f, 100f)]
        float maxSpeed = 10;

        [SerializeField, Range(10f, 100f)]
        float maxAccerlation = 10;

        [SerializeField, Range(0f, 10f)]
        float jumpHeight = 2f;

        [SerializeField, Range(0, 5)]
        int maxAirJumps = 1;

        [SerializeField, Range(0, 90)]
        float maxGroundAngle = 25;

        Vector3 velocity = Vector3.zero;
        Vector3 desiredVelocity = Vector3.zero;

        Rigidbody body;

        bool desiredJump = false;
        bool onGround => groundContactCount > 0;
        int jumpPhase;
        Vector3 contactNormal;
        float minGroundDotProduct;
        float groundContactCount;

        void Awake() 
        {
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        // Start is called before the first frame update
        void Start()
        {
            body = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 playerInput;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            desiredJump |= Input.GetButtonDown("Jump");

            playerInput = Vector2.ClampMagnitude(playerInput, 1f);

            // 计算出期望速度
            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

            GetComponent<Renderer>().material.SetColor("_Color", Color.white * (groundContactCount * 0.25f));
            Debug.Log(groundContactCount);
        }

        void FixedUpdate()
        {
            UpdateState();
            AdjustVelocity();

            if (desiredJump)
            {
                desiredJump = false;
                Jump();
            }

            // 注意：这里onground一直设置为false
            body.velocity = velocity;
            ClearState();
        }

        void ClearState() 
        {
            groundContactCount = 0;
            contactNormal = Vector3.zero;
        }

        /// <summary>
        /// 这里的移动方式其实会导致，物体在斜坡上时，物理重力计算的速度会被逻辑一直向0计算
        /// 导致玩家没有输入时，斜坡上的物体会以一个非常慢的速度向下移动
        /// </summary>
        void AdjustVelocity()
        {
            // 加速度
            float accerlation = onGround ? maxAccerlation : maxAirAccerlation;
            float maxSpeedChange = accerlation * Time.deltaTime;

            // 首先将
            Vector3 xAxis = ProjectOnPlane(Vector3.right);
            Vector3 zAxis = ProjectOnPlane(Vector3.forward);

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            // 计算速度
            float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        Vector3 ProjectOnPlane(Vector3 vector) 
        {
            return vector - contactNormal * Vector3.Dot(contactNormal, vector);
        }

        void UpdateState()
        {
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

        void Jump()
        {
            if ((onGround && jumpPhase == 0) || (!onGround && jumpPhase <= maxAirJumps))
            {
                jumpPhase += 1;
                float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
                // 计算物体速度投影到交点法线上的值
                float alignSpeed = Vector3.Dot(velocity, contactNormal);
                if (alignSpeed > 0)
                {
                    jumpSpeed = Mathf.Max(jumpSpeed - alignSpeed, 0);
                }
                
                // 这里跳跃时，是垂直于当前平面跳跃，因此如果在斜坡上，就需要给物体法线方向上的速度
                velocity += contactNormal * jumpSpeed;
            }
        }

        void OnCollisionEnter(Collision other) {
            EvaluateCollision(other);
        }

        // OnCollisionStay会在FixedUpdate之后执行
        // 因此这里如果球体在plane上，就会将onGround设置为true，为下一次FixedUpdate里的判断做准备
        void OnCollisionStay(Collision other) {
           EvaluateCollision(other);
        }

        void EvaluateCollision(Collision other)
        {
            for (int i = 0; i < other.contactCount; i++)
            {
                Vector3 normal = other.GetContact(i).normal;

                // 判断是否认为该碰撞体是地面
                if (normal.y > minGroundDotProduct)
                {
                    groundContactCount += 1;
                    contactNormal += normal;
                }
            }
        }
    }
}

