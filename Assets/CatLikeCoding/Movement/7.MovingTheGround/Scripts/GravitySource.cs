using UnityEngine;

namespace MovingTheGround
{
    public class GravitySource : MonoBehaviour
    {
        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Vector3.zero;
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