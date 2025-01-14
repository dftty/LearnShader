using UnityEngine;

namespace MovingTheGround
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField, Range(0, 100f)]
        float maxSpeed = 20f;

        [SerializeField, Range(0, 100)]
        float maxSnapSpeed = 10f;

        [SerializeField, Range(0, 100)]
        float maxAcceleration = 10, maxAirAcfeleration = 1f;

        [SerializeField, Range(0, 90)]
        float maxGroundAngle = 25f, maxStairsAngle = 50f;

        [SerializeField, Range(0, 10)]
        float jumpHeight = 2f;

        [SerializeField, Range(0, 5)]
        float maxAirJump = 1;

        [SerializeField]
        LayerMask probeMask, stairsMask;

        [SerializeField, Min(0f)]
        float probeDistance = 1f;

        [SerializeField]
        Transform playerInputSpace;

        int stepsSinceLastGrounded;
        int stepsSinceLastJump;

        float minGroundDotProduct;
        float minStairsDotProduct;

        int jumpPhase;
        bool desireJump;

        bool onGround => groundContactCount > 0;
        int groundContactCount = 0;
        Vector3 contactNormal;

        bool onSteep => steepContactCount > 0;
        int steepContactCount;
        Vector3 steepNormal;

        Vector3 velocity;
        Vector3 desireVelocity;
        Vector3 connectionVelocity;

        Vector3 connectionWorldPosition, connectionLocalPosition;

        Vector3 gravity;
        Vector3 upAxis, rightAxis, forwardAxis;

        Rigidbody body, connectedBody, previousConnectedBody;

        void Start()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        void Update()
        {
            Vector2 playerInput = Vector2.zero;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");

            desireJump |= Input.GetButtonDown("Jump");

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

            desireVelocity = new Vector3(playerInput.x, 0, playerInput.y) * maxSpeed;
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

            velocity += gravity * Time.deltaTime;
            body.velocity = velocity;
            ClearState();
        }

        void UpdateState()
        {
            stepsSinceLastGrounded += 1;
            stepsSinceLastJump += 1;
            velocity = body.velocity;

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

            if (onGround)
            {
                jumpDirection = contactNormal;
            }
            else if (onSteep)
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
            Vector3 xAxis = ProjectOnPlane(rightAxis, contactNormal);
            Vector3 zAxis = ProjectOnPlane(forwardAxis, contactNormal);

            Vector3 relativeVelocity = velocity - connectionVelocity;
            float currentX = Vector3.Dot(relativeVelocity, xAxis);
            float currentZ = Vector3.Dot(relativeVelocity, zAxis);

            float acceleration = onGround ? maxAcceleration : maxAirAcfeleration;
            float maxSpeedChange = acceleration * Time.deltaTime;

            float newX = Mathf.MoveTowards(currentX, desireVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desireVelocity.z, maxSpeedChange);

            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        void ClearState()
        {
            groundContactCount = 0;
            contactNormal = Vector3.zero;

            steepContactCount = 0;
            steepNormal = Vector3.zero;

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
            float minDot = GetMinDot(other.gameObject.layer);

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
                else if (upDot > -0.001f)
                {
                    steepContactCount += 1;
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