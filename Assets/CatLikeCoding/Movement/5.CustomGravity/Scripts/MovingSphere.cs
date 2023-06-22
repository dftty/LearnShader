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
        Vector3 upAxis, forwardAxis, rightAxis;

        void Start()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            maxGroundedAngle = Mathf.Cos(maxGroundedAngle * Mathf.Deg2Rad);
            minStairDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
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
                forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
                rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            }
            else 
            {
                forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
                rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            }

            desireSpeed = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
        }

        void FixedUpdate()
        {
            Vector3 gravity = CustomGravity1.GetGravity(body.position, out upAxis);
            UpdateState();
            AdjustVelocity();

            if (desireJump)
            {
                desireJump = false;
                Jump(gravity);
            }

            velocity += gravity * Time.deltaTime;
            body.velocity = velocity;
            ClearState();
        }

        void Jump(Vector3 gravity)
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
            jumpDirection = (jumpDirection + upAxis).normalized;
            float jumpSpeed = Mathf.Sqrt(2 * gravity.magnitude * jumpHeight);
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
                contactNormal = upAxis;
            }
        }

        bool CheckSteepContact()
        {
            if (steepContactCount > 0)
            {
                steepContactNormal.Normalize();

                float upDot = Vector3.Dot(steepContactNormal, upAxis);
                if (upDot > minGroundedDotProduct)
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

            if (!Physics.Raycast(transform.position, -upAxis, out var hit, probeDistance, probeMask))
            {
                return false;
            }

            float upDot = Vector3.Dot(hit.normal, upAxis);
            if (upDot < GetMinDot(hit.collider.gameObject.layer))
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

            Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
            Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            float newX = Mathf.MoveTowards(currentX, desireSpeed.x, acceleration * Time.deltaTime);
            float newZ = Mathf.MoveTowards(currentZ, desireSpeed.z, acceleration * Time.deltaTime);

            velocity += (newX - currentX) * xAxis + (newZ - currentZ) * zAxis; 
        }

        Vector3 ProjectDirectionOnPlane(Vector3 vec, Vector3 normal)
        {
            return (vec - normal * Vector3.Dot(normal, vec)).normalized;
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
                float upDot = Vector3.Dot(normal, upAxis);
                if (upDot > GetMinDot(other.gameObject.layer))
                {
                    groundContactCount += 1;
                    contactNormal += normal;
                }
                else if (upDot > -0.001f)
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