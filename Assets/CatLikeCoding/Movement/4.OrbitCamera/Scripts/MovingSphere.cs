using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OribitCamera
{

    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(10f, 100f)]
        float maxSpeed = 10, maxSnapSpeed = 20;
        [SerializeField, Range(10f, 100f)]
        float maxAccerlation = 10f, maxAirAccerlation = 1f;
        [SerializeField, Range(2f, 5f)]
        float jumpHeight = 2f;
        [SerializeField, Range(0f, 90f)]
        float maxGroundAngle = 25, maxStairAngle;
        [SerializeField, Range(0, 5)]
        int maxAirJump = 1;
        // 地面射线检测最大距离
        [SerializeField, Range(0f, 1f)]
        float probeDistance = 1;
        // 地面射线检测层级
        [SerializeField]
        LayerMask probeMask, stairMask;
        
        Vector3 desireSpeed;
        Vector3 velocity;
        bool onGround => groundContactCount > 0;
        bool onSteep => steepContactCount > 0;
        int groundContactCount;
        Vector3 contactNormal;

        int steepContactCount;
        Vector3 steepContactNormal;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        float minGroundDotProduct;
        float minStairDotProduct;

        bool desireJump = false;
        int jumpPhase;
        Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 playerInput;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");

            desireJump |= Input.GetButtonDown("Jump");

            playerInput = Vector2.ClampMagnitude(playerInput, 1);
            desireSpeed = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
        }

        void FixedUpdate() 
        {
            UpdateState();
            AdjustVelocity();

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
                jumpPhase = 0;
            }
            else if (maxAirJump > 0 && (jumpPhase <= maxAirJump))
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
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
            float alignSpeed = Vector3.Dot(contactNormal, velocity);
            if (alignSpeed > jumpSpeed)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignSpeed, 0);
            }

            stepsSinceLastJump = 0;
            velocity += contactNormal * jumpSpeed;
        }

        void AdjustVelocity()
        {
            float accerlation = Time.deltaTime * (onGround ? maxAccerlation : maxAirAccerlation);

            Vector3 xAxis = ProjectOnPlane(Vector3.right);
            Vector3 zAxis = ProjectOnPlane(Vector3.forward);

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            float newX = Mathf.MoveTowards(currentX, desireSpeed.x, accerlation);
            float newZ = Mathf.MoveTowards(currentZ, desireSpeed.z, accerlation);
            
            velocity += (newX - currentX) * xAxis + (newZ - currentZ) * zAxis;
        }

        void UpdateState()
        {
            velocity = body.velocity;
            stepsSinceLastGrounded++;
            stepsSinceLastJump++;

            if (onGround || SnapToGround() || CheckSteepContact())
            {
                stepsSinceLastGrounded = 0;
                contactNormal.Normalize();
                
                if (stepsSinceLastJump > 1)
                {
                    jumpPhase = 0;
                }
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

            steepContactCount = 0;
            steepContactNormal = Vector3.zero;
        }

        bool CheckSteepContact()
        {
            if (steepContactCount > 1)
            {
                steepContactNormal.Normalize();

                if (steepContactNormal.y > minGroundDotProduct)
                {
                    groundContactCount = 1;
                    contactNormal = steepContactNormal;
                    return true;
                }
            }            
            return false;
        }

        bool SnapToGround()
        {
            if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
            {
                return false;
            }

            float speed = velocity.magnitude;
            if (speed > maxSnapSpeed)
            {
                return false;
            }

            if (!Physics.Raycast(transform.position, Vector3.down, out var hit, probeDistance, probeMask))
            {
                return false;
            }

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
            return true;
        }

        Vector3 ProjectOnPlane(Vector3 vec)
        {
            return vec - contactNormal * Vector3.Dot(contactNormal, vec);
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
                float dot = GetMinDot(other.gameObject.layer);
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

        float GetMinDot(int layer)
        {
            return ((stairMask & (1 << layer)) == 0) ? minGroundDotProduct : minStairDotProduct;
        }
    }

}