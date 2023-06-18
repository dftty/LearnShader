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

        // Start is called before the first frame update
        void Start()
        {
            regularCamera = GetComponent<Camera>();
            focusPoint = focus.position;
            transform.rotation = Quaternion.Euler(orbitAngles);
        }

        void Update() 
        {
        }

        Vector3 lookDirection;
        Vector3 lookPosition;

        // Update is called once per frame
        void LateUpdate()
        {
            UpdateFocusPoint();
            Quaternion lookRotation;
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                lookRotation = Quaternion.Euler(orbitAngles);
            }
            else 
            {
                lookRotation = transform.localRotation;
            }
            // 这里表示将forward旋转到lookRotation表示的角度上
            lookDirection = lookRotation * Vector3.forward;
            lookPosition = focusPoint - lookDirection * distance;

            // 相机看向物体的向量乘以相机近平面长度
            Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
            // 该向量是相机近平面中心点
            Vector3 rectPosition = lookPosition + rectOffset;
            // BoxCast起点设置为物体位置
            Vector3 castFrom = focus.position;
            // BoxCast应该走过的向量
            Vector3 castLine = rectPosition - castFrom;
            // BoxCast长度和方向
            float castDistance = castLine.magnitude;
            Vector3 castDirection = castLine / castDistance;
            if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out var hit, lookRotation, castDistance))
            {
                // 计算碰撞后的相机近平面位置
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

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
                Input.GetAxis("Vertical Camera"),
			    Input.GetAxis("Horizontal Camera")
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