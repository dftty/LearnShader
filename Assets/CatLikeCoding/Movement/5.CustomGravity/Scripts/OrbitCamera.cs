using UnityEngine;

namespace CustomGravity
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField, Range(2, 10)]
        float distance = 5;

        [SerializeField, Range(0, 3)]
        float focusRadis = 1;

        [SerializeField, Range(0, 1)]
        float focusCentering = 0.75f;

        [SerializeField, Range(0, 90)]
        float minVerticalAngle = 25f, maxVerticalAngle = 60f;

        [SerializeField, Range(0, 360f)]
        float rotateSpeed = 90f;

        [SerializeField]
        Transform focus;

        Vector2 orbitAngles = new Vector2(45f, 0);
        Vector3 previourFocusPoint;
        Vector3 focusPoint;
        Camera regularCamera;

        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(0.5f * regularCamera.fieldOfView * Mathf.Deg2Rad);
                halfExtends.x = halfExtends.y * regularCamera.aspect; 
                halfExtends.z = 0;
                return halfExtends;
            }
        }

        void Start()
        {
            regularCamera = GetComponent<Camera>();
            focusPoint = focus.position;
            transform.rotation = Quaternion.Euler(orbitAngles);
            minVerticalAngle = minVerticalAngle > maxVerticalAngle ? maxVerticalAngle : minVerticalAngle;
        }

        void LateUpdate()
        {
            UpdateFocusPoint();
            Quaternion lookRotation;
            if (ManualRotation())
            {
                ConstrainAngles();
                lookRotation = Quaternion.Euler(orbitAngles);
            }
            else 
            {
                lookRotation = transform.rotation;
            }

            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            // 障碍物探测
            Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castFrom = focus.position;
            Vector3 castLine = rectPosition - castFrom;
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine.normalized;

            if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out var hit, lookRotation, castDistance))
            {
                // 首先计算从物体到碰撞点的位置
                rectPosition = castFrom + castDirection * hit.distance; 
                lookPosition = rectPosition - rectOffset;
            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        void UpdateFocusPoint()
        {
            previourFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1;

            if (distance > 0.001f && focusCentering > 0)
            {
                t = Mathf.Pow(1 - focusCentering, Time.deltaTime);
            }

            if (distance > focusRadis)
            {
                t = focusRadis / distance;
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }

        bool ManualRotation()
        {
            Vector2 input = new Vector2(
                Input.GetAxis("Vertical Camera"),
                Input.GetAxis("Horizontal Camera")
            );

            // 输入阈值
            float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                orbitAngles += rotateSpeed * input * Time.unscaledDeltaTime;
                return true;
            }

            return false;
        }

        void ConstrainAngles()
        {
            orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            // clamp y到360
            if (orbitAngles.y < 0)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f;
            }
        }
    }
}