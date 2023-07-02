using UnityEngine;

namespace MovingTheGround
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
        float rotateSpeed = 90f;

        [SerializeField, Range(0, 90)]
        float minVerticalAngle = 20f, maxVerticalAngle = 60f;

        [SerializeField, Min(0f)]
        float delayTime = 5f;

        [SerializeField, Range(0, 90)]
        float alignSmoothRange = 45f;

        [SerializeField]
        Transform focus;

        Vector2 orbitAngle = new Vector2(45, 0);
        Vector3 focusPoint;
        Vector3 previousFocusPoint;
        Vector3 input;
        float lastManualRotationTime;

        void Start()
        {
            focusPoint = focus.position;
            transform.rotation = Quaternion.Euler(orbitAngle);
        }

        void LateUpdate()
        {
            UpdateFocusPoint();
            Quaternion lookRotation;
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                lookRotation = Quaternion.Euler(orbitAngle);
            }
            else 
            {
                lookRotation = transform.rotation;
            }

            Vector3 lookPosition = focusPoint - transform.forward * distance;
            transform.SetPositionAndRotation(lookPosition, lookRotation);
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
            orbitAngle.x = Mathf.Clamp(orbitAngle.x, minVerticalAngle, maxVerticalAngle);

            if (orbitAngle.y < 0f)
            {
                orbitAngle.y += 360f;
            }
            else if (orbitAngle.y > 360f)
            {
                orbitAngle.y -= 360f;
            }
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

            float movementSqrt = movement.sqrMagnitude;
            if (movementSqrt < 0.001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementSqrt));
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(orbitAngle.y, headingAngle));
            float rotationChange = rotateSpeed * Time.unscaledDeltaTime;

            // 旋转在做小距离平滑时，因为有正向旋转以及反向旋转，因此需要判断两个
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

        float GetAngle(Vector2 dir)
        {
            float angle = Mathf.Acos(dir.y) * Mathf.Rad2Deg;
            return dir.x > 0 ? angle : 360 - angle;
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
                orbitAngle += input * Time.unscaledDeltaTime * rotateSpeed;
                lastManualRotationTime = Time.unscaledTime;
                return true;
            }

            return false;
        }
    }
}