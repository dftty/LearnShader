using UnityEngine;

namespace Rolling
{
    public class MovingSphereHead : MonoBehaviour
    {
        [SerializeField]
        float distance = 1f;

        [SerializeField]
        MovingSphere sphere;

        [SerializeField]
        LayerMask headMask;

        Vector3 halfExtends
        {
            get
            {
                Vector3 extends;
                extends.x = sphere.radius * 0.5f;
                extends.y = extends.x;
                extends.z = 0;
                return extends;
            }
        }

        void FixedUpdate()
        {
            Vector3 up = sphere.transform.up;
            Vector3 position = sphere.transform.position + up * distance;

            if (Physics.BoxCast(sphere.transform.position, halfExtends, up, out var hit,  sphere.transform.rotation, distance, headMask))
            {
                position = sphere.transform.position + up * Mathf.Max(0, hit.distance - 0.1f);
            }

            Debug.Log(hit.collider);

            transform.position = position;
        }
    }
}