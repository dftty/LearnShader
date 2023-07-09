using UnityEngine;

namespace Swimming
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField]
        Transform focus;

        [SerializeField, Min(0)]
        float distance = 5f;

        [SerializeField, Min(0)]
        float focusRadis = 1f;

        [SerializeField, Range(0, 1)]
        float focusCentering = 0.5f;

        [SerializeField, Range(0, 360)]
        float rotationSpeed = 360f;

        [SerializeField, Range(0, 90)]
        float minVerticalAngle = 20f, maxVerticalAngle = 70f;

        [SerializeField]
        float delayTime = 5f;

        [SerializeField, Range(0, 90)]
        float alignSmoothRange = 45f;

        [SerializeField]
        LayerMask obstructionMask;

        float lastManualRotationTime = 0;

        Vector3 focusPoint;
        Vector3 previoutFocusPoint;
        Vector2 orbitAngle = new Vector2(45, 0);

        Camera orbitCamera;

        Vector3 CameraHalfEntends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y = Mathf.Tan(0.5f * orbitCamera.fieldOfView * Mathf.Deg2Rad) * orbitCamera.nearClipPlane;
                halfExtends.x = halfExtends.y * orbitCamera.aspect;
                halfExtends.z = 0;
                return halfExtends;
            }
        }

        void Start()
        {
            orbitCamera = GetComponent<Camera>();
            transform.rotation = Quaternion.Euler(orbitAngle);
        }

        void LateUpdate()
        {
            UpdateFocusPoint();

            Quaternion lookRotation;
            if (ManualRotation() || AutoRotation())
            {
                ConstrainAngles();
                lookRotation = Quaternion.Euler(orbitAngle);
            }
            else 
            {
                lookRotation = transform.rotation;
            }

            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            Vector3 rectOffset = lookDirection * orbitCamera.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focus.position;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine / castDistance;

            if (Physics.BoxCast(castFrom, CameraHalfEntends, castDirection, out var hit, lookRotation, castDistance, obstructionMask))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        bool ManualRotation()
        {
            Vector2 input = new Vector2(
                Input.GetAxis("Vertical Camera"),
			    Input.GetAxis("Horizontal Camera")
            );

            const float e = 0.001f;

            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                orbitAngle += rotationSpeed * input * Time.unscaledDeltaTime;
                lastManualRotationTime = Time.unscaledTime;
                return true;
            }

            return false;
        }

        bool AutoRotation()
        {
            if (Time.unscaledTime - lastManualRotationTime < delayTime)
            {
                return false;
            }

            Vector2 movement = new Vector2(
                focusPoint.x - previoutFocusPoint.x,
                focusPoint.z - previoutFocusPoint.z
            );

            float sqrMagnitude = movement.sqrMagnitude;
            if (sqrMagnitude < 0.001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(sqrMagnitude));
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(orbitAngle.y, headingAngle));
            float rotationChange = rotationSpeed * Time.unscaledDeltaTime;

            if (deltaAngle < alignSmoothRange)
            {
                rotationChange *= deltaAngle / alignSmoothRange;
            }
            else if (180 - deltaAngle < alignSmoothRange)
            {
                rotationChange *= (180 - deltaAngle) / alignSmoothRange;
            }

            orbitAngle.y = Mathf.MoveTowardsAngle(orbitAngle.y, headingAngle, rotationChange);
            return true;
        }

        void ConstrainAngles()
        {
            orbitAngle.x = Mathf.Clamp(orbitAngle.x, minVerticalAngle, maxVerticalAngle);

            if (orbitAngle.y <= 0)
            {
                orbitAngle.y += 360f;
            }
            else if (orbitAngle.y >= 360)
            {
                orbitAngle.y -= 360f;
            }
        }

        float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x > 0 ? angle : 360 - angle;
        }

        void UpdateFocusPoint()
        {
            previoutFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;
            float t = 1;
            float distance = Vector3.Distance(targetPoint, focusPoint);

            if (distance > 0.01f && focusCentering > 0)
            {
                t = Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime);
            }

            if (distance > focusRadis)
            {
                t = focusRadis / distance;
            }

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
    }
}