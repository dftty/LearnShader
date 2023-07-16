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

        [SerializeField]
        float maxSwimSpeed = 5f;

        [SerializeField, Range(0, 100)]
        float maxAcceleration = 20, maxAirAcceleration = 2, maxClimbAcceleration = 10, maxSwimAcceleration = 5f;

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
        LayerMask probeMask, stairMask, climbMask, waterMask;

        [SerializeField]
        Transform playerInputSpace;

        [SerializeField]
        Material normalMaterial, climbMaterila, swimMaterial;

        [SerializeField]
        float submergeOffset = 0.5f;

        [SerializeField]
        float submergeRange = 1f;   

        // 水阻力系数
        [SerializeField, Range(0, 10f)]
        float waterDrag = 1f;

        // 浮力系数
        [SerializeField, Min(0f)]
        float buoyancy = 1;

        [SerializeField, Range(0.01f, 1)]
        float swimThreshold = 0.5f;

        bool InWater => submerge > 0;
        float submerge;
        bool Swimming => submerge >= swimThreshold;

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

        bool desiresClimbing;
        bool desiredJump;
        int jumpPhase;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        Vector3 rightAxis, forwardAxis, upAxis;

        Vector3 gravity;

        Vector3 playerInput;
        Vector3 desireVelocity;
        Vector3 velocity;
        Vector3 connectionVelocity;
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
            playerInput.z = Swimming ? Input.GetAxis("UpDown") : 0;

            if (Swimming)
            {
                desiresClimbing = false;
            }
            else 
            {
                desiredJump |= Input.GetButtonDown("Jump");
                desiresClimbing = Input.GetButton("Climb");
            }

            playerInput = Vector3.ClampMagnitude(playerInput, 1);

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

            GetComponent<MeshRenderer>().material = Climbing ? climbMaterila : Swimming ? swimMaterial : normalMaterial;
        }

        void FixedUpdate()
        {
            gravity = CustomGravity.GetGravity(transform.position, out upAxis);
            UpdateState();

            if (InWater)
            {
                velocity *= 1 - waterDrag * submerge * Time.deltaTime;
            }

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
            else if (InWater)
            {
                velocity += gravity * ((1 - buoyancy * submerge) * Time.deltaTime);
            }
            else if (desiresClimbing && OnGround)
            {
                // 这里是当在爬墙时，要冲出墙面到达地面时，给物体一个倾向于平面移动的力，否则物体会掉下去
                velocity += (gravity - contactNormal * maxClimbAcceleration * 0.9f) * Time.deltaTime;
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

            if (CheckClimb() || CheckSwimming() || OnGround || SnapToGround() || CheckSteepContact())
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

        bool CheckSwimming()
        {
            if (Swimming)
            {
                groundContactCount = 0;
                contactNormal = upAxis;
                return true;
            }

            return false;
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
                connectionVelocity = pos / Time.deltaTime;
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

            if (!Physics.Raycast(transform.position, -upAxis, out var hit, probeDistance, probeMask, QueryTriggerInteraction.Ignore))
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
            else if (Swimming)
            {
                // 当在水中时，如果没有完全进入水下，那么加速度应该在地面加速度和水下加速度之间
                float swimFactor = Mathf.Min(1, submerge / swimThreshold);
                xAxis = rightAxis;
                zAxis = forwardAxis;
                speed = Mathf.LerpUnclamped(maxSpeed, maxSwimSpeed, swimFactor);
                acceleration = Mathf.LerpUnclamped(OnGround ? maxAcceleration : maxAirAcceleration, maxSwimAcceleration, swimFactor);
            }
            else 
            {
                xAxis = rightAxis;
                zAxis = forwardAxis;
                acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
                speed = OnGround && desiresClimbing ? maxClimbSpeed : maxSpeed;
            }

            xAxis = ProjectOnPlane(xAxis, contactNormal);
            zAxis = ProjectOnPlane(zAxis, contactNormal);
            float maxSpeedChange = acceleration * Time.deltaTime;

            Vector3 relativeVelocity = velocity - connectionVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float newX = Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

            velocity += (newX - currentX) * xAxis + (newZ - currentZ) * zAxis;

            if (Swimming)
            {
                float currentY = Vector3.Dot(relativeVelocity, upAxis);
                float newY = Mathf.MoveTowards(currentY, playerInput.z * speed, maxSpeedChange);
                velocity += (newY - currentY) * upAxis;
            }
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

            submerge = 0;
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

            if (InWater)
            {
                jumpSpeed *= Mathf.Max(0, 1f - submerge / swimThreshold);
            }

            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            jumpDirection = (jumpDirection + upAxis).normalized;

            if (alignedSpeed > 0)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0);
            }

            velocity += jumpDirection * jumpSpeed;
        }

        void OnTriggerEnter(Collider other) 
        {
            if ((waterMask & 1 << other.gameObject.layer) != 0)
            {
                EvaluateSubmerge(other);
            }
        }

        void OnTriggerStay(Collider other) 
        {
            if ((waterMask & 1 << other.gameObject.layer) != 0)
            {
                EvaluateSubmerge(other);
            }
        }

        void EvaluateSubmerge(Collider other)
        {
            if (Physics.Raycast(
                body.position + upAxis * submergeOffset, 
                -upAxis, out var hit, submergeRange + 1, 
                waterMask, QueryTriggerInteraction.Collide))
            {
                submerge = 1 - hit.distance / submergeRange;
            }
            else 
            {
                submerge = 1;
            }

            if (Swimming)
            {
                connectedBody = other.attachedRigidbody;
            }
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
            if (Swimming)
            {
                return ;
            }

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

                    if (desiresClimbing && upDot > minClimbDotProduct && (climbMask & (1 << layer)) != 0)
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
			//Gizmos.DrawLine(Vector3.zero, upAxis);
        }
    }
}