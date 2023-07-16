using UnityEngine;

namespace Swimming
{
    public class CustomGravityRigidbody : MonoBehaviour
    {
        
        [SerializeField]
        bool floatToSleep = false;

        [SerializeField]
        float submergeOffset = 0.5f;

        [SerializeField]
        float submergeRange = 1f;

        [SerializeField]
        float waterDrag = 1f;

        [SerializeField]
        LayerMask waterMask;

        [SerializeField]
        Vector3 buoyancyOffset = Vector3.zero;

        [SerializeField]
        float buoyancy = 1f;

        float submergence;

        Vector3 gravity;

        Rigidbody body;

        float floatDelay;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
        }

        void FixedUpdate()
        {
            if (floatToSleep)
            {
                if (body.IsSleeping())
                {
                    floatDelay = 0f;
                    return ;
                }

                if (body.velocity.sqrMagnitude < 0.0001f)
                {
                    floatDelay += Time.deltaTime;
                    if (floatDelay >= 1f)
                    {
                        return ;
                    }
                }
                else 
                {
                    floatDelay = 0;
                }
            }

            gravity = CustomGravity.GetGravity(body.position);
            if (submergence > 0)
            {
                float drag = Mathf.Max(0, 1 - waterDrag * submergence * Time.deltaTime);
                body.velocity *= drag;
                body.angularVelocity *= drag;

                body.AddForceAtPosition(gravity * -(buoyancy * submergence), transform.TransformPoint(buoyancyOffset), ForceMode.Acceleration);
                submergence = 0;
            }

            body.AddForce(gravity, ForceMode.Acceleration);
        }

        void OnTriggerEnter(Collider other) {
            if ((waterMask & (1 << other.gameObject.layer)) != 0)
            {
                EvaluateSubmergency();
            }
        }

        void OnTriggerStay(Collider other) {
            if (!body.IsSleeping() && (waterMask & (1 << other.gameObject.layer)) != 0)
            {
                EvaluateSubmergency();
            }
        }

        void EvaluateSubmergency()
        {
            Vector3 upAxis = -gravity.normalized;

            if (Physics.Raycast(body.position + upAxis * submergeOffset, -upAxis, out var hit, submergeRange + 1, waterMask, QueryTriggerInteraction.Collide))
            {
                submergence = 1 - hit.distance / submergeRange;
            }
            else 
            {
                submergence = 1;
            }
        }
    }
}