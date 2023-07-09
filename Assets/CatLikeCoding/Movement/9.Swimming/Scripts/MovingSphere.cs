using UnityEngine;

namespace Swimming
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0, 100)]
        float maxSpeed = 10;

        [SerializeField, Range(0, 100)]
        float maxSnapSpeed = 10;

        [SerializeField, Range(0, 100)]
        float maxAcceleration = 20, maxAirAcceleration = 2;

        [SerializeField, Range(0, 90)]
        float maxGroundAngle = 40;

        [SerializeField, Range(0, 90)]
        float maxStairsAngle = 50;

        [SerializeField, Range(0, 10)]
        float jumpHeight = 3f;

        [SerializeField, Range(0, 5)]
        float maxAirJump = 1;

        [SerializeField, Range(0, 1)]
        float probeDistance = 1;

        [SerializeField]
        LayerMask probeMask, stairMask;

        [SerializeField]
        Transform playerInputSpace;

        float minGroundDotProduct;
        float minStairsDotProduct;

        bool OnGround => groundContactCount > 0;
        int groundContactCount;
        Vector3 contactNormal;

        bool OnSteep => steepContactCount > 0;
        int steepContactCount;
        Vector3 steepNormal;

        bool desiredJump;
        int jumpPhase;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        Vector3 rightAxis, forwardAxis;

        Vector3 desireVelocity;
        Vector3 velocity;
        Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        void Update()
        {
            Vector2 playerInput;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");

            desiredJump |= Input.GetButtonDown("Jump");

            playerInput = Vector2.ClampMagnitude(playerInput, 1);

            if (playerInputSpace)
            {
                rightAxis = playerInputSpace.right;
                forwardAxis = playerInputSpace.forward;
            }
            else 
            {
                rightAxis = Vector3.right;
                forwardAxis = Vector3.forward;
            }

            desireVelocity = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
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

            body.velocity = velocity;
            ClearState();
        }

        void UpdateState()
        {
            velocity = body.velocity;
            stepsSinceLastGrounded++;
            stepsSinceLastJump++;

            if (OnGround || SnapToGround() || CheckSteepContact())
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

            groundContactCount = 1;
            contactNormal = hit.normal;

            float dot = Vector3.Dot(velocity, contactNormal);
            if (dot > 0)
            {
                velocity = (velocity - contactNormal * dot).normalized * speed;
            }

            return true;
        }

        bool CheckSteepContact()
        {
            if (steepContactCount > 1)
            {
                steepNormal.Normalize();
                if (steepNormal.y > minGroundDotProduct)
                {
                    groundContactCount = 1;
                    contactNormal = steepNormal;
                }
            }

            return false;
        }

        void AdjustVelocity()
        {
            Vector3 xAxis = ProjectOnPlane(rightAxis, contactNormal);
            Vector3 zAxis = ProjectOnPlane(forwardAxis, contactNormal);

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX = Mathf.MoveTowards(currentX, desireVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desireVelocity.z, maxSpeedChange);

            velocity += (newX - currentX) * xAxis + (newZ - currentZ) * zAxis;
        }

        Vector3 ProjectOnPlane(Vector3 vec, Vector3 normal)
        {
            return (vec - Vector3.Dot(vec, normal) * normal).normalized;
        }

        void ClearState()
        {
            groundContactCount = 0;
            contactNormal = Vector3.zero;

            steepContactCount = 0;
            steepNormal = Vector3.zero;
        }

        void Jump()
        {
            Vector3 jumpDirection;

            if (OnGround)
            {
                jumpDirection = contactNormal;
            }
            else if (OnSteep)
            {
                jumpDirection = steepNormal;
                jumpPhase = 0;
            }
            else if (maxAirJump > 0 && jumpPhase < maxAirJump)
            {
                jumpDirection = contactNormal;

                if (jumpPhase == 0)
                {
                    jumpPhase = 1;
                }
            }
            else 
            {
                return ;
            }

            jumpPhase += 1;
            stepsSinceLastJump = 0;
            float jumpSpeed = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            jumpDirection = (jumpDirection + Vector3.up).normalized;

            Debug.Log(jumpDirection);

            if (alignedSpeed > 0)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0);
            }

            velocity += jumpDirection * jumpSpeed;
        }

        void  OnCollisionEnter(Collision other) 
        {
            EvaluteCollision(other);
        }

        void OnCollisionStay(Collision other)
        {
            EvaluteCollision(other);
        }

        void EvaluteCollision(Collision collision)
        {
            int layer = collision.gameObject.layer;
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;

                if (normal.y > GetMinDot(layer))
                {
                    groundContactCount++;
                    contactNormal += normal;
                }
                else if (normal.y > -0.001f)
                {
                    steepContactCount++;
                    steepNormal += normal;
                }
            }
        }

        float GetMinDot(int layer)
        {
            return (stairMask & 1 << layer) == 0 ? minGroundDotProduct : minStairsDotProduct;
        }
    }
}