using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OribitCamera
{
    [RequireComponent(typeof(Camera))]
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField]
        Transform focus;

        [SerializeField, Min(0f)]
        float focusRadis = 1f;

        [SerializeField, Range(1f, 10f)]
        float distance = 2f;

        [SerializeField, Range(0f, 1f)]
        float focusCentering = 0.5f;

        Vector3 focusPoint;

        // Start is called before the first frame update
        void Start()
        {
            focusPoint = focus.position;
        }

        void Update() 
        {
            
        }

        // Update is called once per frame
        void LateUpdate()
        {
            UpdateFocusPoint();
            Vector3 lookDirection = transform.forward;
            transform.position = focusPoint - lookDirection * distance;
        }

        void UpdateFocusPoint()
        {
            Vector3 targetPoint = focus.position;
            float t = 1;
            float distance = Vector3.Distance(focusPoint, targetPoint);
            if (distance > 0.01f && focusCentering > 0)
            {
                t = Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime);

                Debug.Log(t);
            }

            if (distance > focusRadis)
            {
                t = Mathf.Min(t, focusRadis / distance);
            }

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
    }

}