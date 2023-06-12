# 简单相机跟随制作

## 1.跟随物体移动

物体的位置计算代码如下:
```csharp
float distance = 5;

Vector3 forward = transform.forward;
transform.position = focus.transform.position - forward * distance
```

### 1.1 在靠近跟随物体一定距离时，插值慢慢靠近

核心函数如下
```csharp
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
```