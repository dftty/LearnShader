using UnityEngine;

namespace ReactiveEnvironment
{
    public class AccelerationZone : MonoBehaviour
    {
        [SerializeField]
        float speed = 10f, acceleration = 10;

        void OnTriggerEnter(Collider other) 
        {
            Rigidbody body = other.attachedRigidbody;

            if (body)
            {
                Accelerate(body);
            }    
        }

        void OnTriggerStay(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;

            if (body)
            {
                Accelerate(body);
            }  
        }

        void Accelerate(Rigidbody body)
        {
            Vector3 velocity = transform.InverseTransformDirection(body.velocity);

            if (velocity.y >= speed)
            {
                return ;
            }

            if (acceleration > 0)
            {
                velocity.y = Mathf.MoveTowards(velocity.y, speed, acceleration * Time.deltaTime);
            }
            else 
            {
                velocity.y = speed;
            }

            body.velocity = transform.TransformDirection(velocity);

            if (body.gameObject.TryGetComponent<MovingSphere>(out var comp))
            {
                comp.PreventSnapToGround();
            }
        }
    }
}