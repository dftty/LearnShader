using UnityEngine;

namespace Climbing
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0, 100f)]
        float maxSpeed = 20f;

        [SerializeField, Range(0, 100)]
        float maxSnapSpeed = 10f;

        [SerializeField, Range(0, 10)]
        float maxClimbSpeed = 2f;

        [SerializeField, Range(0, 100)]
        float maxAcceleration = 10, maxAirAcceleration = 1f, maxClimbAcceleration = 20f;

        [SerializeField, Range(0, 90)]
        float maxGroundAngle = 25f, maxStairsAngle = 50f;

        [SerializeField, Range(90, 170)]
        float maxClimbAngle = 140f;

        [SerializeField, Range(0, 10)]
        float jumpHeight = 2f;

        [SerializeField, Range(0, 5)]
        float maxAirJump = 1;

        [SerializeField]
        LayerMask probeMask, stairsMask, climbMask;

        [SerializeField, Min(0f)]
        float probeDistance = 1f;

        [SerializeField]
        Transform playerInputSpace;

        [SerializeField]
        Material normalMaterial, climbMaterial;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        float minGroundDotProduct;
        float minStairsDotProduct;
        float minClimbDotProduct;

        int jumpPhase;
        bool desireJump;
        bool desiresClimbing;

        bool OnGround => groundContactCount > 0;
        int groundContactCount = 0;
        Vector3 contactNormal;

        bool OnSteep => steepContactCount > 0;
        int steepContactCount;
        Vector3 steepNormal;

        bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;
        int climbContactCount;
        Vector3 climbNormal;

        Vector2 playerInput;
        Vector3 velocity;
        Vector3 connectionVelocity;

        Vector3 connectionWorldPosition, connectionLocalPosition;

        Vector3 gravity;
        Vector3 upAxis, rightAxis, forwardAxis;

        Rigidbody body, connectedBody, previousConnectedBody;

        MeshRenderer meshRenderer;

        void Start()
        {
            body = GetComponent<Rigidbody>();
            meshRenderer = GetComponent<MeshRenderer>();
            body.useGravity = false;
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
            minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
        }

        void Update()
        {
            playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");

            desireJump |= Input.GetButtonDown("Jump");
            desiresClimbing = Input.GetButton("Climb");

            // 将input的长度限制到1
            playerInput = Vector2.ClampMagnitude(playerInput, 1); 

            if (playerInputSpace)
            {
                rightAxis = ProjectOnPlane(playerInputSpace.right, contactNormal);
                forwardAxis = ProjectOnPlane(playerInputSpace.forward, contactNormal);
            }
            else 
            {
                rightAxis = ProjectOnPlane(Vector3.right, contactNormal);
                forwardAxis = ProjectOnPlane(Vector3.forward, contactNormal);
            }

            meshRenderer.material = Climbing ? climbMaterial : normalMaterial;
        }

        void FixedUpdate()
        {
            gravity = CustomGravity.GetGravity(transform.position, out upAxis);

            UpdateState();
            AdjustVelocity();

            if (desireJump)
            {
                desireJump = false;
                Jump(gravity);
            }

            if (Climbing)
            {
                // 如果在墙面上移动，那么给球体一个向墙面方向掉落的速度
                velocity -= contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
            }
            else if (OnGround && velocity.sqrMagnitude < 0.01f)
            {
                velocity += contactNormal * (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
            }

            else if (desiresClimbing && OnGround)
            {
                velocity += (gravity - contactNormal * maxAcceleration * 0.9f) * Time.deltaTime;
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
            stepsSinceLastGrounded += 1;
            stepsSinceLastJump += 1;
            velocity = body.velocity;

            if (CheckClimbing() || OnGround || SnapToGround() || CheckSteepContact())
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

            if (connectedBody)
            {
                if (connectedBody.isKinematic && connectedBody.mass >= body.mass)
                {
                    UpdateConnectedVelocity();
                }
            }
        }

        void UpdateConnectedVelocity()
        {
            if (connectedBody == previousConnectedBody)
            {
                Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
                connectionVelocity = connectionMovement / Time.deltaTime;
            }

            connectionWorldPosition = body.position;
            connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
        }

        bool CheckClimbing()
        {
            if (Climbing)
            {
                groundContactCount = climbContactCount;
                contactNormal = climbNormal;
                return true;
            }

            return false;
        }

        bool CheckSteepContact()
        {
            if (steepContactCount > 1)
            {
                steepNormal.Normalize();
                if (steepNormal.y >= maxGroundAngle)
                {
                    groundContactCount = 1;
                    contactNormal = steepNormal;
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

            if (!Physics.Raycast(body.position, gravity.normalized, out RaycastHit hit, probeDistance, probeMask))
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

            // 消除y轴的速度
            float dot = Vector3.Dot(contactNormal, velocity);
            if (dot > 0)
            {
                velocity = (velocity - contactNormal * dot).normalized * speed;
            }

            connectedBody = hit.rigidbody;
            return true;
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

            float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
            stepsSinceLastJump = 0;

            float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
            if (alignedSpeed > 0)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0);
            }
            jumpDirection = (jumpDirection + upAxis).normalized;

            velocity += jumpDirection * jumpSpeed;
        }

        void AdjustVelocity()
        {
            Vector3 xAxis, zAxis;
            
            float acceleration, speed;

            if (Climbing) {
                acceleration = maxClimbAcceleration;
                speed = maxClimbSpeed;
                xAxis = Vector3.Cross(contactNormal, upAxis);
                zAxis = upAxis;
            }
            else {
                acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
                speed = OnGround && desiresClimbing ? maxClimbSpeed : maxSpeed;
                xAxis = rightAxis;
                zAxis = forwardAxis;
            }
            xAxis = ProjectOnPlane(xAxis, contactNormal);
            zAxis = ProjectOnPlane(zAxis, contactNormal);
            
            Vector3 relativeVelocity = velocity - connectionVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX =
                Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
            float newZ =
                Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        void ClearState()
        {
            groundContactCount = 0;
            contactNormal = Vector3.zero;

            steepContactCount = 0;
            steepNormal = Vector3.zero;

            climbContactCount = 0;
            climbNormal = Vector3.zero;

            previousConnectedBody = connectedBody;
            connectedBody = null;
        }

        Vector3 ProjectOnPlane(Vector3 vector, Vector3 normal)
        {
            return (vector - Vector3.Dot(vector, normal) * normal).normalized;
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
            int layer = other.gameObject.layer;
            float minDot = GetMinDot(layer);

            for (int i = 0; i < other.contactCount; i++)
            {
                Vector3 normal = other.GetContact(i).normal;
                float upDot = Vector3.Dot(normal, upAxis);
                if (upDot >= minDot)
                {
                    groundContactCount += 1;
                    contactNormal += normal;
                    connectedBody = other.rigidbody;
                }
                else 
                {
                    if (upDot > -0.001f)
                    {
                        steepContactCount += 1;
                        steepNormal += normal;

                        if (connectedBody == null)
                        {
                            connectedBody = other.rigidbody;
                        }
                    }

                    if (desiresClimbing && upDot >= minClimbDotProduct && (climbMask & (1 << layer)) != 0)
                    {
                        climbContactCount += 1;
                        climbNormal += normal;
                        connectedBody = other.rigidbody;
                    }
                }
            }
        }

        float GetMinDot(int layer)
        {
            return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
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