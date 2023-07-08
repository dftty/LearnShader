using UnityEngine;

namespace Climbing
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField, Range(1f, 20f)]
        float distance = 5f;

        [SerializeField, Min(0f)]
        float focusRadius = 1f;
        
        [SerializeField, Range(0, 1)]
        float focusCentering = 0.5f;

        [SerializeField, Range(0, 360f)]
        float rotationSpeed = 90f;

        [SerializeField, Range(0, 90)]
        float minVerticalAngle = 20f, maxVerticalAngle = 60f;

        [SerializeField, Min(0f)]
        float delayTime = 5f;

        [SerializeField, Range(0, 90)]
        float alignSmoothRange = 45f;

        [SerializeField, Min(0)]
        float upAlignmentSpeed = 360f;  

        [SerializeField]
        Transform focus;

        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends = Vector3.zero;
                halfExtends.y = regularCamera.nearClipPlane * Mathf.Atan(regularCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                halfExtends.x = regularCamera.aspect * halfExtends.x;
                halfExtends.z = 0;
                return halfExtends;
            }
        }

        Camera regularCamera;

        Vector2 orbitAngles = new Vector2(45, 0);
        Vector3 focusPoint;
        Vector3 previousFocusPoint;
        Vector3 input;
        float lastManualRotationTime;

        Quaternion gravityAlignment = Quaternion.identity;
        Quaternion orbitRotation;

        void Start()
        {
            focusPoint = focus.position;
            transform.rotation = Quaternion.Euler(orbitAngles);
            regularCamera = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            UpdateGravityAligment();
            UpdateFocusPoint();
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                orbitRotation = Quaternion.Euler(orbitAngles);
            }

            Quaternion lookRotation = gravityAlignment * orbitRotation;

            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focus.position;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine / castDistance;

            if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out var hit, lookRotation, castDistance))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        void UpdateGravityAligment()
        {
            Vector3 fromUp = gravityAlignment * Vector3.up;
            Vector3 toUp = CustomGravity.GetUpAxis(focus.position);

            // 计算两个向量之间的夹角
            float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1, 1);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

            float maxAngle = upAlignmentSpeed * Time.deltaTime;
            Quaternion newAligment = Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;

            if (angle < maxAngle)
            {
                gravityAlignment = newAligment;
            }
            else 
            {
                gravityAlignment = Quaternion.SlerpUnclamped(gravityAlignment, newAligment, maxAngle / angle);
            }
        }

        void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;
            float t = 1;
            float distance = Vector3.Distance(previousFocusPoint, focus.position);

            if (distance > 0.01f && focusCentering > 0)
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

            if (orbitAngles.y < 0f)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y > 360f)
            {
                orbitAngles.y -= 360f;
            }
        }

        bool AutomaticRotation()
        {
            if (Time.unscaledTime - lastManualRotationTime < delayTime)
            {
                return false;
            }

            Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previousFocusPoint);
            Vector2 movement = new Vector2(
                alignedDelta.x,
                alignedDelta.z
            );

            float movementSqrt = movement.sqrMagnitude;
            if (movementSqrt < 0.001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementSqrt));
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            float rotationChange = rotationSpeed * Time.unscaledDeltaTime;

            // 旋转在做小距离平滑时，因为有正向旋转以及反向旋转，因此需要判断两个
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

        bool ManualRotation()
        {
            Vector2 input = new Vector2(
                Input.GetAxis("Vertical Camera"),
                Input.GetAxis("Horizontal Camera")
            );

            // 判断是否有输入
            const float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                orbitAngles += input * Time.unscaledDeltaTime * rotationSpeed;
                lastManualRotationTime = Time.unscaledTime;
                return true;
            }

            return false;
        }
    }
}