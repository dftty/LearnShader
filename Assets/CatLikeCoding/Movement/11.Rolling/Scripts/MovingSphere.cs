using UnityEngine;

namespace Rolling
{
    public class MovingSphere : MonoBehaviour
    {   
        [SerializeField]
        Transform inputSpace;

        [SerializeField, Range(0, 20)]
        float maxSpeed = 10, maxSnapSpeed = 20, maxClimbSpeed = 5, maxSwimSpeed = 5f;


        [SerializeField, Range(0, 20)]
        float maxAcceleration = 20, maxAirAcceleration = 2f, maxClimbAcceleration = 10f, maxSwimAcceleration = 5f;

        [SerializeField, Range(0, 90)]
        float maxGroundAngle = 40f, maxStairAngle = 50f;

        [SerializeField, Range(90, 170)]
        float maxClimbAngle = 140f;

        [SerializeField, Range(0, 10)]
        float jumpHeight = 2f;

        [SerializeField, Range(0, 5)]
        int maxAirJump = 0;

        [SerializeField, Range(0, 1)]
        float probeDistance = 1;

        [SerializeField]
        LayerMask probeMask, stairsMask, climbMask, waterMask;

        [SerializeField]
        float submergeOffset = 0.5f;

        [SerializeField]
        float submergeRange = 1f;

        [SerializeField, Range(0, 10f)]
        float waterDrag = 1f;

        [SerializeField, Min(0f)]
        float buoyancy = 1;

        [SerializeField, Range(0.01f, 1)]
        float swimThreshold = 0.5f;

        bool OnGround => groundedContactCount > 0;
        int groundedContactCount;
        Vector3 contactNormal;

        bool OnSteep => steepContactCount > 0;
        int steepContactCount;
        Vector3 steepNormal;

        bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;
        int climbContactCount;
        Vector3 climbNormal;

        bool InWater => submerge > 0;
        float submerge;
        bool Swimming => submerge >= swimThreshold;

        Rigidbody body, connectedBody, previousConnectedBody;

        Vector3 velocity;
        Vector3 connectionVelocity;

        Vector3 connectionWorldPosition, connectionLocalPosition;

        Vector3 playerInput;
        bool desireJump;
        int jumpPhase;

        bool desiresClimbing;

        float minGroundDotProduct;
        float minStairsDotProduct;
        float minClimbDotProduct;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        Vector3 rightAxis, forwardAxis, upAxis;

        void Start()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
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
                desireJump |= Input.GetButtonDown("Jump");
                desiresClimbing = Input.GetButton("Climb");
            }

            playerInput = Vector2.ClampMagnitude(playerInput, 1);

            if (inputSpace)
            {
                rightAxis = inputSpace.right;
                forwardAxis = inputSpace.forward;
            }
            else 
            {
                rightAxis = Vector3.right;
                forwardAxis = Vector3.forward;
            }
        }

        void FixedUpdate()
        {
            Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

            UpdateState();

            if (InWater)
            {
                // 附加水的阻力
                velocity *= 1 - waterDrag * submerge * Time.deltaTime;
            }

            AdjustVelocity();

            if (desireJump)
            {
                desireJump = false;
                Jump(gravity);
            }

            if (Climbing)
            {
                velocity -= (contactNormal * maxClimbAcceleration * 0.9f) * Time.deltaTime;
            }
            else if (InWater)
            {
                velocity += gravity * ((1 - buoyancy * submerge) * Time.deltaTime);
            }
            else if (desiresClimbing && velocity.magnitude < 0.001f)
            {
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

            if (CheckClimbing() || CheckSwimming() || OnGround || SnapToGround() || SteepContactCheck())
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

        bool CheckSwimming()
        {
            if (Swimming)
            {
                groundedContactCount = 0;
                contactNormal = upAxis;
                return true;
            }

            return false;
        }

        bool CheckClimbing()
        {
            if (climbContactCount > 0)
            {
                groundedContactCount = climbContactCount;
                contactNormal = climbNormal;
                return true;
            }

            return false;
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

            if (Swimming)
            {
                jumpSpeed *= Mathf.Max(0, 1f - submerge / swimThreshold);
            }

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
            Vector3 xAxis, zAxis;
            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            float speed;

            if (Climbing)
            {
                xAxis = Vector3.Cross(contactNormal, upAxis);
                zAxis = upAxis;
                acceleration = maxClimbAcceleration;
                speed = maxClimbSpeed;
            }
            else if (Swimming)
            {
                float swimFactor = Mathf.Min(1, submerge / swimThreshold);
                xAxis = rightAxis;
                zAxis = forwardAxis;
                speed = Mathf.LerpUnclamped(maxSpeed, maxSwimSpeed, swimFactor);
                acceleration = Mathf.LerpUnclamped(maxAcceleration, maxSwimAcceleration, swimFactor);
            }
            else 
            {
                xAxis = rightAxis;
                zAxis = forwardAxis;
                acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
                speed = maxSpeed;
            }

            xAxis = ProjectOnPlane(xAxis, contactNormal);
            zAxis = ProjectOnPlane(zAxis, contactNormal);

            Vector3 relativeVelocity = velocity - connectionVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float maxSpeedChange = acceleration * Time.deltaTime;
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
            groundedContactCount = 0;
            contactNormal = Vector3.zero;

            steepContactCount = 0;
            steepNormal = Vector3.zero;

            climbContactCount = 0;
            climbNormal = Vector3.zero;

            previousConnectedBody = connectedBody;
            connectedBody = null;

            submerge = 0;
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
            if (Swimming)
            {
                return ;
            }

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
                else 
                {
                    if (upDot > -0.001f)
                    {
                        steepContactCount++;
                        steepNormal += normal;

                        if (connectedBody == null)
                        {
                            connectedBody = other.rigidbody;
                        }
                    }
                    
                    if (desiresClimbing && (climbMask & (1 << layer)) != 0 && upDot > minClimbDotProduct)
                    {
                        climbContactCount++;
                        climbNormal += normal;
                        connectedBody = other.rigidbody;
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if ((waterMask & (1 << other.gameObject.layer)) != 0)
            {
                EvaluateSumberge(other);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if ((waterMask & (1 << other.gameObject.layer)) != 0)
            {
                EvaluateSumberge(other);
            }
        }

        void EvaluateSumberge(Collider other)
        {
            // 从物体头部向下发射射线
            if (Physics.Raycast(body.position + upAxis * submergeOffset, -upAxis, out var hit, submergeRange + 1, waterMask, QueryTriggerInteraction.Collide))
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

        float GetMinDot(int layer)
        {
            return (stairsMask & (1 << layer)) != 0 ? minStairsDotProduct : minGroundDotProduct;
        }

        void OnDrawGizmos()
		{
			if (!Application.isPlaying)
			{
				return ;
			}

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