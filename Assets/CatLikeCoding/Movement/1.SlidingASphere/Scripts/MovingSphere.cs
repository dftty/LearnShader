using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  SlidingASphere
{
    public class MovingSphere : MonoBehaviour
    {

        [SerializeField, Range(10f, 100f)]
        float maxSpeed = 10;

        [SerializeField, Range(10f, 100f)]
        float maxAccerlation = 10;

        [SerializeField]
        Rect allowArea = new Rect(-5f, -5f, 10f, 10f);

        [SerializeField, Range(0, 1f)]
        float bounciness = 0.5f;

        Vector3 velocity = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 playerInput;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");

            playerInput = Vector2.ClampMagnitude(playerInput, 1f);

            // 计算出期望速度
            Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
            // 加速度
            float maxSpeedChange = maxAccerlation * Time.deltaTime;

            // 计算速度
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            Vector3 newPosition = transform.position + velocity * Time.deltaTime;
            // 限制移动范围并增加弹性
            if (newPosition.x < allowArea.xMin)
            {
                newPosition.x = allowArea.xMin;
                velocity.x = -velocity.x * bounciness;
            }
            else if (newPosition.x > allowArea.xMax)
            {
                newPosition.x = allowArea.xMax;
                velocity.x = -velocity.x * bounciness;
            }

            if (newPosition.z < allowArea.yMin)
            {
                newPosition.z = allowArea.yMin;
                velocity.z = -velocity.z * bounciness;
            }
            else if (newPosition.z > allowArea.yMax)
            {
                newPosition.z = allowArea.yMax;
                velocity.z = -velocity.z * bounciness;
            }

            transform.position = newPosition;
        }
    }
}

