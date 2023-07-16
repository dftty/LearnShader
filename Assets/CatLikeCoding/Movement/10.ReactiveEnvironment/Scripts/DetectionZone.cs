using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace ReactiveEnvironment
{
    public class DetectionZone : MonoBehaviour
    {
        [SerializeField]
        UnityEvent onFirstEnter, onLastExit;

        List<Collider> colliders = new List<Collider>();

        void Awake()
        {
            enabled = false;
        }

        void FixedUpdate()
        {
            for (int i = 0; i < colliders.Count; i++)
            {
                Collider collider = colliders[i];

                if (!collider || !collider.gameObject.activeInHierarchy)
                {
                    colliders.RemoveAt(i--);

                    if (colliders.Count == 0)
                    {
                        onLastExit.Invoke();
                        enabled = false;
                    }
                }
            }
        }

        // trigger函数不论是否enable都会触发
        private void OnTriggerEnter(Collider other) {
            if (colliders.Count == 0)
            {
                onFirstEnter.Invoke();
                enabled = true;
            }

            colliders.Add(other);
        }

        private void OnTriggerExit(Collider other) {
            if (colliders.Remove(other) && colliders.Count == 0)
            {
                onLastExit.Invoke();
                enabled =  false;
            }
        }

        void OnDisable() {

#if UNITY_EDITOR
            if (enabled && gameObject.activeInHierarchy)
            {
                return ;
            }
#endif

            if (colliders.Count > 0)
            {
                colliders.Clear();
                onLastExit.Invoke();
            }
        }
    }
}