using UnityEngine;

namespace Rolling
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField]
        Transform focus;

        [SerializeField, Min(0)]
        float distance = 5;

        [SerializeField, Range(0, 5)]
        float focusRadius = 1;

        [SerializeField, Range(0, 1)]
        float focusCentering = 0.5f;

        [SerializeField, Range(0, 720)]
        float rotationSpeed = 360f;

        [SerializeField, Range(0f, 90f)]
        float alignSmoothRange = 45f;

        [SerializeField]
        float alignDelay = 5f;

        [SerializeField, Range(-45, 90)]
        float minVerticalAngle = -20f, maxVerticalAngle = 70f;

        [SerializeField]
        LayerMask obstructionMask;

        [SerializeField]
        float upAlignmentSpeed = 360f;

        Quaternion gravityAlignment = Quaternion.identity;
        Quaternion orbitRotation;

        Vector3 focusPoint, previousFocusPoint;
        Vector2 orbitAngles = new Vector2(45, 0);
        float lastManualRotationTime;

        Camera orbitCamera;
        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y = orbitCamera.nearClipPlane * Mathf.Tan(0.5f * orbitCamera.fieldOfView * Mathf.Deg2Rad);
                halfExtends.x = orbitCamera.aspect * halfExtends.y;
                halfExtends.z = 0;
                return halfExtends;
            }
        }

        void Awake()
        {
            orbitCamera = GetComponent<Camera>();
            transform.rotation = orbitRotation = Quaternion.Euler(orbitAngles);
        }

        void LateUpdate()
        {
            UpdateGravityAlignment();
            UpdateFocusTarget();

            if (ManualRotation() || AutoRotation())
            {
                ConstrainAngles();
                orbitRotation = Quaternion.Euler(orbitAngles);
            }

            Quaternion lookRotation = gravityAlignment * orbitRotation;
            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            Vector3 rectOffset = lookDirection * orbitCamera.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focus.position;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine.normalized;

            if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out var hit, lookRotation, castDistance, obstructionMask, QueryTriggerInteraction.Ignore))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        void UpdateGravityAlignment()
        {
            Vector3 fromUp = gravityAlignment * Vector3.up;
            Vector3 toUp = CustomGravity.GetUpAxis(focus.position);

            // 该计算角度方式与下面相同
            // float angle = Vector3.Angle(fromUp, toUp);
            float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1, 1);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            float maxAngle = upAlignmentSpeed * Time.deltaTime;

            Quaternion newGravityAligment = Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;

            if (angle < maxAngle)
            {
                gravityAlignment = newGravityAligment;
            }
            else 
            {
                gravityAlignment = Quaternion.SlerpUnclamped(gravityAlignment, newGravityAligment, maxAngle / angle);
            }
        }

        void UpdateFocusTarget()
        {
            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;

            float distance = Vector3.Distance(focusPoint, targetPoint);
            float t = 1;

            if (distance > 0.001f && focusCentering > 0)
            {
                t = Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime);
            }

            if (distance > focusRadius)
            {
                t = focusRadius / distance;
            }

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }

        void ConstrainAngles()
        {
            orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f;
            }
            else if (orbitAngles.y < 0)
            {
                orbitAngles.y += 360f;
            }
        }

        bool ManualRotation()
        {
            Vector2 input = new Vector2(
                Input.GetAxis("Vertical Camera"),
                Input.GetAxis("Horizontal Camera")
            );

            const float e = 0.0001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                orbitAngles += input * rotationSpeed * Time.deltaTime;
                lastManualRotationTime = Time.unscaledTime;
                return true;
            }

            return false;
        }

        bool AutoRotation()
        {
            if (Time.unscaledTime - lastManualRotationTime < alignDelay)
            {
                return false;
            }

            Vector3 alignedDelta =
			    Quaternion.Inverse(gravityAlignment) *
			    (focusPoint - previousFocusPoint);
            
		    Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.0001f) 
            {
                return false;
            }

            float headingAngle = GetAngle(movement.normalized);
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(headingAngle, orbitAngles.y));
            float rotationChange =
			    rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

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

        float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x > 0 ? angle : 360 - angle;
        }
    }
}