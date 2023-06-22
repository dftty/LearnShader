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

        [SerializeField, Range(1, 10)]
        float delayTime = 5f;

        [SerializeField, Range(0f, 90f)]
        float alignSmoothRange = 45f;

        [SerializeField]
        Transform focus;

        Vector2 orbitAngles = new Vector2(45f, 0);
        Vector3 previourFocusPoint;
        Vector3 focusPoint;
        Camera regularCamera;
        float lastManualRotationTime;
        Quaternion gravityAlignment = Quaternion.identity;
        Quaternion orbitRotation;

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
            transform.rotation = orbitRotation = Quaternion.Euler(orbitAngles);
            minVerticalAngle = minVerticalAngle > maxVerticalAngle ? maxVerticalAngle : minVerticalAngle;
        }

        void LateUpdate()
        {
            gravityAlignment = Quaternion.FromToRotation(
                gravityAlignment * Vector3.up, 
                CustomGravity1.GetUpAxis(focusPoint)
                ) * gravityAlignment;

            UpdateFocusPoint();
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                orbitRotation = Quaternion.Euler(orbitAngles);
            }

            Quaternion lookRotation = gravityAlignment * orbitRotation;
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

        bool AutomaticRotation()
        {
            if (Time.unscaledTime - lastManualRotationTime < delayTime)
            {
                return false;
            }

            // 跟随的物体相对上一帧的位移
            Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previourFocusPoint);
            Vector2 movement = new Vector2(
                alignedDelta.x, alignedDelta.z
            );

            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.001f)
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
                lastManualRotationTime = Time.unscaledTime;
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

        /// <summary>
        /// 该函数计算的是xy坐标轴内，该向量和y轴之间的夹角
        /// 计算出角度后，还应该根据x值的正负进行判断
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        static float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x > 0 ? angle : 360 - angle;
        }
    }
}