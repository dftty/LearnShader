using UnityEngine;

namespace Rolling
{
    public class CustomGravityRigidBody : MonoBehaviour
    {
        
        [SerializeField]
        float submergeOffset = 0.5f;

        [SerializeField]
        float submergeRange = 1f;

        [SerializeField, Range(1, 10)]
        float waterDrag = 1;

        [SerializeField]
        float buoyance = 1f;

        [SerializeField]
        LayerMask waterLayer;

        [SerializeField]
        Vector3 buoyancyOffset;

        bool InWater => submergence > 0;

        float submergence;

        Rigidbody body;

        Vector3 gravity;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
        }

        void FixedUpdate()
        {
            gravity = CustomGravity.GetGravity(body.position);

            Debug.Log(submergence);
            if (InWater)
            {
                float dragFactor = Mathf.Max(0, 1 - waterDrag * submergence * Time.deltaTime);
                body.velocity *= dragFactor;
                body.angularVelocity *= dragFactor;

                body.AddForceAtPosition(gravity * -(buoyance * submergence), transform.TransformPoint(buoyancyOffset), ForceMode.Acceleration);
                submergence = 0;
            }

            body.AddForce(gravity, ForceMode.Acceleration);
        }

        void OnTriggerEnter(Collider other)
        {
            if ((waterLayer * 1 << other.gameObject.layer) != 0)
            {
                EvaluteSubmergence(other);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if ((waterLayer * 1 << other.gameObject.layer) != 0)
            {
                EvaluteSubmergence(other);
            }
        }

        void EvaluteSubmergence(Collider other)
        {
            Vector3 upAxis = -gravity.normalized;

            if (Physics.Raycast(body.position + upAxis * submergeOffset, -upAxis, out var hit, submergeRange + 1, waterLayer, QueryTriggerInteraction.Collide))
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