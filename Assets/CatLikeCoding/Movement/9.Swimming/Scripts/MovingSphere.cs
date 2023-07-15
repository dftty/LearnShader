using UnityEngine;

namespace Swimming
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0, 100)]
        float maxSpeed = 10;

        [SerializeField, Range(0, 100)]
        float maxSnapSpeed = 10;

        [SerializeField]
        float maxClimbSpeed = 2;

        [SerializeField, Range(0, 100)]
        float maxAcceleration = 20, maxAirAcceleration = 2, maxClimbAcceleration = 10;

        [SerializeField, Range(0, 90)]
        float maxGroundAngle = 40;

        [SerializeField, Range(0, 90)]
        float maxStairsAngle = 50;

        [SerializeField, Range(90, 170)]
        float maxClimbAngle = 140;

        [SerializeField, Range(0, 10)]
        float jumpHeight = 3f;

        [SerializeField, Range(0, 5)]
        float maxAirJump = 1;

        [SerializeField, Range(0, 1)]
        float probeDistance = 1;

        [SerializeField]
        LayerMask probeMask, stairMask, climbMask;

        [SerializeField]
        Transform playerInputSpace;

        [SerializeField]
        Material normalMaterial, climbMaterila;

        float minGroundDotProduct;
        float minStairsDotProduct;
        float minClimbDotProduct;

        bool OnGround => groundContactCount > 0;
        int groundContactCount;
        Vector3 contactNormal;

        bool OnSteep => steepContactCount > 0;
        int steepContactCount;
        Vector3 steepNormal;

        bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;
        int climbContactCount;
        Vector3 climbNormal;

        bool desireClimbing;
        bool desiredJump;
        int jumpPhase;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        Vector3 rightAxis, forwardAxis, upAxis;

        Vector3 gravity;

        Vector2 playerInput;
        Vector3 desireVelocity;
        Vector3 velocity;
        Vector3 connectionBodyVelocity;
        Rigidbody body, connectedBody, previoutConnectedBody;

        Vector3 connectionWorldPosition, connectionLocalPosition;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
            minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
        }

        void Update()
        {
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");

            desiredJump |= Input.GetButtonDown("Jump");
            desireClimbing = Input.GetButton("Climb");

            playerInput = Vector2.ClampMagnitude(playerInput, 1);

            if (playerInputSpace)
            {
                rightAxis = ProjectOnPlane(playerInputSpace.right, upAxis);
                forwardAxis = ProjectOnPlane(playerInputSpace.forward, upAxis);
            }
            else 
            {
                rightAxis = ProjectOnPlane(Vector3.right, upAxis);
                forwardAxis = ProjectOnPlane(Vector3.forward, upAxis);
            }

            GetComponent<MeshRenderer>().material = Climbing ? climbMaterila : normalMaterial;
        }

        void FixedUpdate()
        {
            gravity = CustomGravity.GetGravity(transform.position, out upAxis);
            UpdateState();
            AdjustVelocity();

            if (desiredJump)
            {
                desiredJump = false;
                Jump(gravity);
            }

            if (Climbing)
            {
                // 这里如果没有0.9，那么会在爬内角墙时卡住
                velocity -= contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
            }
            else if (desireClimbing && OnGround)
            {
                velocity += ((gravity - contactNormal) * maxClimbAcceleration * 0.9f) * Time.deltaTime;
            }
            else 
            {
                velocity += gravity * Time.deltaTime;
            }
            
            body.velocity = velocity;
            ClearState();
        }

        void UpdateState()
        {
            velocity = body.velocity;
            stepsSinceLastGrounded++;
            stepsSinceLastJump++;

            if (CheckClimb() || OnGround || SnapToGround() || CheckSteepContact())
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
                contactNormal = CustomGravity.GetUpAxis(transform.position);
            }

            if (connectedBody)
            {
                if (connectedBody.isKinematic && connectedBody.mass >= body.mass)
                {
                    UpdateConnectedBodyVelocity();
                }
            }
        }

        bool CheckClimb()
        {
            if (Climbing)
            {
                groundContactCount = climbContactCount;
                contactNormal = climbNormal;

                return true;
            }

            return false;
        }

        void UpdateConnectedBodyVelocity()
        {
            if (previoutConnectedBody == connectedBody)
            {
                Vector3 pos = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
                connectionBodyVelocity = pos / Time.deltaTime;
            }

            connectionWorldPosition = body.position;
            connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
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

            connectedBody = hit.rigidbody;
            return true;
        }

        bool CheckSteepContact()
        {
            if (steepContactCount > 1)
            {
                steepNormal.Normalize();
                float upDot = Vector3.Dot(steepNormal, upAxis);
                if (upDot > minGroundDotProduct)
                {
                    groundContactCount = 1;
                    contactNormal = steepNormal;
                }
            }

            return false;
        }

        void AdjustVelocity()
        {
            Vector3 xAxis;
            Vector3 zAxis;

            float acceleration, speed;
            if (Climbing)
            {
                xAxis = Vector3.Cross(contactNormal, upAxis);
                zAxis = upAxis;
                acceleration = maxClimbAcceleration;
                speed = maxClimbSpeed;
            }
            else 
            {
                xAxis = rightAxis;
                zAxis = forwardAxis;
                acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
                speed = OnGround && desireClimbing ? maxClimbSpeed : maxSpeed;
            }

            xAxis = ProjectOnPlane(xAxis, contactNormal);
            zAxis = ProjectOnPlane(zAxis, contactNormal);
            float maxSpeedChange = acceleration * Time.deltaTime;

            Vector3 relativeVelocity = velocity - connectionBodyVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float newX = Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

            Debug.Log(playerInput);

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

            climbContactCount = 0;
            climbNormal = Vector3.zero;

            previoutConnectedBody = connectedBody;
            connectedBody = null;
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
            float jumpSpeed = Mathf.Sqrt(2 * gravity.magnitude * jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            jumpDirection = (jumpDirection + upAxis).normalized;

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

                float upDot = Vector3.Dot(normal, upAxis);
                if (upDot > GetMinDot(layer))
                {
                    groundContactCount++;
                    contactNormal += normal;
                    connectedBody = collision.rigidbody;
                }
                else 
                {
                    if (upDot > -0.001f)
                    {
                        steepContactCount++;
                        steepNormal += normal;

                        if (connectedBody == null)
                        {
                            connectedBody = collision.rigidbody;
                        }
                    }

                    if (desireClimbing && upDot > minClimbDotProduct && (climbMask & (1 << layer)) != 0)
                    {
                        climbContactCount++;
                        climbNormal += normal;
                    }
                }
            }
        }

        float GetMinDot(int layer)
        {
            return (stairMask & 1 << layer) == 0 ? minGroundDotProduct : minStairsDotProduct;
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.black;
			Gizmos.DrawLine(Vector3.zero, contactNormal);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(Vector3.zero, rightAxis);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(Vector3.zero, forwardAxis);
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(Vector3.zero, upAxis);
        }
    }
}