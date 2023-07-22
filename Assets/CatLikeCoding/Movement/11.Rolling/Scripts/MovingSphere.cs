using UnityEngine;

namespace Rolling
{
    public class MovingSphere : MonoBehaviour
    {   
        [SerializeField]
        Transform inputSpace;

        [SerializeField, Range(0, 20)]
        float maxSpeed = 10, maxSnapSpeed = 20;

        [SerializeField, Range(0, 20)]
        float maxAcceleration = 20, maxAirAcceleration = 2f;

        [SerializeField, Range(0, 90)]
        float maxGroundAngle = 40f, maxStairAngle = 50f;

        [SerializeField, Range(0, 10)]
        float jumpHeight = 2f;

        [SerializeField, Range(0, 5)]
        int maxAirJump = 0;

        [SerializeField, Range(0, 1)]
        float probeDistance = 1;

        [SerializeField]
        LayerMask probeMask, stairsMask;

        bool OnGround => groundedContactCount > 0;
        int groundedContactCount;
        Vector3 contactNormal;

        bool OnSteep => steepContactCount > 0;
        int steepContactCount;
        Vector3 steepNormal;

        Rigidbody body, connectedBody, previousConnectedBody;

        Vector3 velocity;
        Vector3 connectionVelocity;

        Vector3 connectionWorldPosition, connectionLocalPosition;

        Vector3 desiredVelocity;
        bool desireJump;
        int jumpPhase;

        float minGroundDotProduct;
        float minStairsDotProduct;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        Vector3 xAxis, zAxis, upAxis;

        void Start()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
        }

        void Update()
        {
            Vector2 playerInput;

            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            desireJump |= Input.GetButtonDown("Jump");

            playerInput = Vector2.ClampMagnitude(playerInput, 1);

            if (inputSpace)
            {
                xAxis = inputSpace.right;
                zAxis = inputSpace.forward;
            }
            else 
            {
                xAxis = Vector3.right;
                zAxis = Vector3.forward;
            }

            desiredVelocity = new Vector3(playerInput.x * maxSpeed, 0, playerInput.y * maxSpeed);
        }

        void FixedUpdate()
        {
            Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

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

        void UpdateState()
        {
            velocity = body.velocity;
            stepsSinceLastGrounded++;
            stepsSinceLastJump++;

            if (OnGround || SnapToGround() || SteepContactCheck())
            {
                stepsSinceLastGrounded = 0;
                if (stepsSinceLastJump > 1)
                {
                    jumpPhase = 0;
                }
                contactNormal.Normalize();
            }
            else 
            {
                contactNormal = upAxis;
            }

            if (connectedBody)
            {
                if (connectedBody.isKinematic && connectedBody.mass >= body.mass)
                {
                    UpdateConnectedBody();
                }
            }
        }

        void UpdateConnectedBody()
        {
            if (previousConnectedBody == connectedBody)
            {
                Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
                connectionVelocity = connectionMovement / Time.deltaTime;
            }

            connectionWorldPosition = body.position;
            connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
        }

        bool SteepContactCheck()
        {
            if (steepContactCount > 0)
            {
                steepNormal.Normalize();

                if (steepNormal.y > minGroundDotProduct)
                {
                    groundedContactCount = steepContactCount;
                    contactNormal = steepNormal;
                }
            }

            return false;
        }

        void Jump(Vector3 gravity)
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
            else if (maxAirJump > 0 && jumpPhase <= maxAirJump)
            {
                jumpDirection = contactNormal;
            }
            else 
            {
                return ;
            }

            jumpDirection = (jumpDirection + upAxis).normalized;
            stepsSinceLastJump = 0;
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(2 * gravity.magnitude * jumpHeight);
            float alignSpeed = Vector3.Dot(velocity, contactNormal);

            if (alignSpeed > 0)
            {
                jumpSpeed = Mathf.Max(0, jumpSpeed - alignSpeed);
            }

            velocity += jumpDirection * jumpSpeed;
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

            if (!Physics.Raycast(transform.position, -upAxis, out var hit, probeDistance, probeMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            float upDot = Vector3.Dot(upAxis, hit.normal);
            if (upDot < GetMinDot(hit.transform.gameObject.layer))
            {
                return false;
            }

            groundedContactCount = 1;
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
            xAxis = ProjectOnPlane(xAxis, contactNormal);
            zAxis = ProjectOnPlane(zAxis, contactNormal);

            Vector3 relativeVelocity = velocity - connectionVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            velocity += (newX - currentX) * xAxis + (newZ - currentZ) * zAxis;
        }

        Vector3 ProjectOnPlane(Vector3 vec, Vector3 normal)
        {
            return (vec - Vector3.Dot(vec, normal) * normal).normalized;
        }

        void ClearState()
        {
            groundedContactCount = 0;
            contactNormal = Vector3.zero;

            steepContactCount = 0;
            steepNormal = Vector3.zero;

            previousConnectedBody = connectedBody;
            connectedBody = null;
        }

        void OnCollisionEnter(Collision other)
        {
            EvaluteCollision(other);
        }

        void OnCollisionStay(Collision other)
        {
            EvaluteCollision(other);
        }

        void EvaluteCollision(Collision other)
        {
            int layer = other.gameObject.layer;
            float minDot = GetMinDot(layer);
            for (int i = 0; i < other.contactCount; i++)
            {
                Vector3 normal = other.GetContact(i).normal;
                float upDot = Vector3.Dot(upAxis, normal);

                if (upDot >= minDot)
                {
                    groundedContactCount++;
                    contactNormal += normal;
                    connectedBody = other.rigidbody;
                }
                else if (upDot > -0.001f)
                {
                    steepContactCount++;
                    steepNormal += normal;

                    if (connectedBody == null)
                    {
                        connectedBody = other.rigidbody;
                    }
                }
            }
        }

        float GetMinDot(int layer)
        {
            return (stairsMask & (1 << layer)) != 0 ? minStairsDotProduct : minGroundDotProduct;
        }
    }
}