using UnityEngine;

namespace Swimming
{
    public class StableFloatingRigidbody : MonoBehaviour
    {
        
        [SerializeField]
        float submergeOffset = 0.5f;

        [SerializeField]
        float submergeRange = 1;

        [SerializeField, Range(0, 10)]
        float waterDrag = 1;

        [SerializeField, Min(0)]
        float buoyancy = 1;

        [SerializeField]
        LayerMask waterMask;

        [SerializeField]
        Vector3[] buoyancyOffsets;

        [SerializeField]
        bool safeFloating = false;

        float[] submergence;

        Rigidbody body;

        Vector3 gravity;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;

            submergence = new float[buoyancyOffsets.Length];
        }

        void FixedUpdate()
        {

            gravity = CustomGravity.GetGravity(body.position);

            
            float dragFactor = waterDrag * Time.deltaTime / buoyancyOffsets.Length;
            float buoyancyFactor = -buoyancy / buoyancyOffsets.Length;

            for (int i = 0; i < buoyancyOffsets.Length; i++)
            {
                if (submergence[i] > 0)
                {
                    float drag = Mathf.Max(0, 1 - dragFactor * submergence[i]);
                    body.velocity *= drag;
                    body.angularVelocity *= drag;


                    body.AddForceAtPosition(gravity * buoyancyFactor * submergence[i], transform.TransformPoint(buoyancyOffsets[i]), ForceMode.Acceleration);
                    submergence[i] = 0;
                }
            }
            

            body.AddForce(gravity, ForceMode.Acceleration);
        }

        private void OnTriggerEnter(Collider other) {
            if ((waterMask & (1 << other.gameObject.layer)) != 0)
            {
                EvaluateSubmergency();
            }
        }

        private void OnTriggerStay(Collider other) {
            if (!body.IsSleeping() && (waterMask & (1 << other.gameObject.layer)) != 0)
            {
                EvaluateSubmergency();
            }
        }

        void EvaluateSubmergency()
        {
            Vector3 down = gravity.normalized;
            Vector3 offset = down * -submergeOffset;

            for (int i = 0; i < buoyancyOffsets.Length; i++)
            {
                Vector3 p = offset + transform.TransformPoint(buoyancyOffsets[i]);
                if (Physics.Raycast(p, down, out var hit, submergeRange + 1, waterMask, QueryTriggerInteraction.Collide))
                {
                    submergence[i] = 1 - hit.distance / submergeRange;
                }
                else if (!safeFloating || Physics.CheckSphere(p, 0.01f, waterMask, QueryTriggerInteraction.Collide))
                {
                    submergence[i] = 1;
                }
            }
            
        }
    }
}