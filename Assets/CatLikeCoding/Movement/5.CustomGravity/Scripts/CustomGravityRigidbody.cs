using UnityEngine;

namespace CustomGravity
{  
    [RequireComponent(typeof(Rigidbody))]
    public class CustomGravityRigidbody : MonoBehaviour
    {
        Rigidbody body;

        float floatDelay;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            // 不受系统重力影响
            body.useGravity = false;
        }

        void FixedUpdate()
        {
            if (body.IsSleeping())
            {
                floatDelay = 0;
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

            body.AddForce(CustomGravity1.GetGravity(transform.position), ForceMode.Acceleration);
        }

    }
}