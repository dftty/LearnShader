using UnityEngine;

namespace ComplexGravity
{
    public class GravitySource : MonoBehaviour
    {
        public Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        void OnEnable()
        {
            CustomGravity.Register(this);
        }

        void OnDisable()
        {
            CustomGravity.UnRegister(this);
        }
    }
}