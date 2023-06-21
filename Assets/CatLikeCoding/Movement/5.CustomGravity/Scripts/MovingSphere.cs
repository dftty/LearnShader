using UnityEngine;

namespace CustomGravity
{
    public class MovingSphere : MonoBehaviour
    {

        [SerializeField, Range(0, 100)]
        float maxSpeed = 30;

        [SerializeField, Range(0, 100)]
        float maxAcceleration = 10, maxAirAcceleration = 1;

        [SerializeField, Range(0, 100)]
        float maxSnapSpeed = 10;

        [SerializeField, Range(0, 90)]
        float maxGroundedAngle = 25f, maxStairAngle = 50f;

        [SerializeField, Range(0, 5)]
        int maxAirJump = 1;

        [SerializeField, Range(0, 5)]
        float jumpHeight = 2f;

        [SerializeField]
        LayerMask stairMask, probeMask;

        [SerializeField]
        float probeDistance = 1;

        [SerializeField]
        Transform playerInputSpace;

        bool desireJump;
        int jumpPhase;
        int groundContactCount = 0;
        Vector3 contactNormal;
        bool onGround => groundContactCount > 0;
        int steepContactCount = 0;
        Vector3 steepContactNormal;
        bool onSteep => steepContactCount > 0;

        float minGroundedDotProduct;
        float minStairDotProduct;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        Vector3 desireSpeed;
        Vector3 velocity;
        Rigidbody body;

        void Start()
        {
            body = GetComponent<Rigidbody>();
            maxGroundedAngle = Mathf.Cos(maxGroundedAngle * Mathf.Deg2Rad);
            minStairDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
            Debug.Log(minStairDotProduct);
        }

        void Update()
        {
            Vector2 playerInput;

            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");

            desireJump |= Input.GetButtonDown("Jump");

            playerInput = Vector2.ClampMagnitude(playerInput, 1f);

            if (playerInputSpace)
            {
                // 因为角色移动的平面是XZ平面，因此将向量的y都设置为0
                Vector3 forward = playerInputSpace.forward;
                forward.y = 0;
                forward.Normalize();
                Vector3 right = playerInputSpace.right;
                right.y = 0;
                right.Normalize();

                desireSpeed = (forward * playerInput.y + right * playerInput.x) * maxSpeed;
            }
            else 
            {
                desireSpeed = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
            }

        }

        void FixedUpdate()
        {
            UpdateState();
            Debug.Log(contactNormal);
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
            else if (maxAirJump > 0 && jumpPhase <= maxAirJump)
            {
                jumpDirection = contactNormal;

                if (jumpPhase == 0)
                {
                    jumpPhase = 1;
                }
            }else 
            {
                return ;
            }

            stepsSinceLastJump = 0;
            jumpPhase += 1;
            jumpDirection = (jumpDirection + Vector3.up).normalized;
            float jumpSpeed = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
            float alignSpeed = Vector3.Dot(velocity, jumpDirection);
            
            if (jumpSpeed > alignSpeed)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignSpeed, 0);
            }
            velocity += jumpDirection * jumpSpeed;
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

        bool CheckSteepContact()
        {
            if (steepContactCount > 0)
            {
                steepContactNormal.Normalize();

                if (steepContactNormal.y > minGroundedDotProduct)
                {
                    groundContactCount = 1;
                    contactNormal = steepContactNormal;
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

        void AdjustVelocity()
        {
            float acceleration = onGround ? maxAcceleration : maxAirAcceleration;

            Vector3 xAxis = ProjectOnPlane(Vector3.right);
            Vector3 zAxis = ProjectOnPlane(Vector3.forward);

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            float newX = Mathf.MoveTowards(currentX, desireSpeed.x, acceleration * Time.deltaTime);
            float newZ = Mathf.MoveTowards(currentZ, desireSpeed.z, acceleration * Time.deltaTime);

            velocity += (newX - currentX) * xAxis + (newZ - currentZ) * zAxis; 
        }

        Vector3 ProjectOnPlane(Vector3 vec)
        {
            return vec - contactNormal * Vector3.Dot(contactNormal, vec);
        }

        void ClearState()
        {
            groundContactCount = 0;
            contactNormal = Vector3.zero;

            steepContactCount = 0;
            steepContactNormal = Vector3.zero;
        }

        void OnCollisionStay(Collision other)
        {
            EvalueateCollision(other);
        }

        void OnCollisionEnter(Collision other)
        {
            EvalueateCollision(other);
        }

        void EvalueateCollision(Collision other)
        {
            for (int i = 0; i < other.contactCount; i++)
            {
                Vector3 normal = other.GetContact(i).normal;

                if (normal.y > GetMinDot(other.gameObject.layer))
                {
                    groundContactCount += 1;
                    contactNormal += normal;
                }
                else if (normal.y > -0.001f)
                {
                    steepContactCount += 1;
                    steepContactNormal += normal;
                }
            }
        }
    
        float GetMinDot(int layer)
        {
            return (stairMask & 1 << layer) == 0 ? minGroundedDotProduct : minStairDotProduct;
        }
    }
}