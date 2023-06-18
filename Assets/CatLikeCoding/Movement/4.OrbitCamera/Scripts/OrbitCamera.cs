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

        [SerializeField, Range(1f, 360f)]
        float rotateSpeed = 90f;

        [SerializeField, Range(-89f, 89f)]
        float minVerticalAngle = -30f, maxVerticalAngle = 60f;

        [SerializeField, Min(0)]
        float delayTime = 5f;

        [SerializeField, Range(0f, 90f)]
        float alignSmoothRange = 45f;

        Vector2 orbitAngles = new Vector2(45f, 0);
        Vector3 focusPoint, previousFocusPoint;
        float lastManualRotationTime;

        // Start is called before the first frame update
        void Start()
        {
            focusPoint = focus.position;
            transform.rotation = Quaternion.Euler(orbitAngles);
        }

        void Update() 
        {
            
        }

        // Update is called once per frame
        void LateUpdate()
        {
            UpdateFocusPoint();
            Quaternion lookRotation;
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                // Debug.Log(orbitAngles);
                lookRotation = Quaternion.Euler(orbitAngles);
            }
            else 
            {
                lookRotation = transform.localRotation;
            }
            Vector3 lookDirection = transform.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;
            float t = 1;
            float distance = Vector3.Distance(focusPoint, targetPoint);
            if (distance > 0.01f && focusCentering > 0)
            {
                t = Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime);
            }

            if (distance > focusRadis)
            {
                t = Mathf.Min(t, focusRadis / distance);
            }

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }

        void ConstrainAngles()
        {
            orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            if (orbitAngles.y < 0f)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f;
            }
        }

        bool ManualRotation()
        {
            Vector2 input = new Vector2(
                Input.GetAxis("Mouse Y"),
                Input.GetAxis("Mouse X")
            );

            const float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                orbitAngles += rotateSpeed * input * Time.unscaledDeltaTime;
                lastManualRotationTime = Time.unscaledTime;
                return true;
            }
            return false;
        }

        bool AutomaticRotation()
        {
            if (Time.unscaledTime - lastManualRotationTime < delayTime)
            {
                return false;
            }

            Vector2 movement = new Vector2(
                focusPoint.x - previousFocusPoint.x,
                focusPoint.z - previousFocusPoint.z
            );

            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.0001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            float rotationChange = rotateSpeed * Time.unscaledDeltaTime;
            if (deltaAngle < alignSmoothRange)
            {
               rotationChange *= deltaAngle / alignSmoothRange;
            }
            else if (180 - deltaAngle < alignSmoothRange)
            {
               rotationChange *= (180 - deltaAngle) / alignSmoothRange;
            }

            orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

            return true;
        }

        void OnValidate() 
        {
            if (maxVerticalAngle < minVerticalAngle)
            {
                maxVerticalAngle = minVerticalAngle;
            }
        }

        static float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x < 0 ? 360 - angle : angle;
        }
    }

}