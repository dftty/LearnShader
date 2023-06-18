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


## 2.相机跟随物体旋转




## 3.前进方向修改为相机方向

可以让MovingSphere持有相机的Transform，然后获取该Transform的forward和right，注意这里需要将这两个向量的y值置为0，相当于投影到了物体移动的地面上，然后归一化
接下来分别用forward和right乘以输入的y和x，然后相加，就得到了物体移动的方向

## 4.相机碰撞检测
原理为从跟随的目标位置开始，向相机进行BoxCast检测，其中Box的大小为相机的nearPlane的大小，深度为0，相当于一个长方形，而不是长方体

### 4.1 相机属性介绍
field of View 视野在垂直方向的角度
aspect 宽度和高度的比值

因为BaxCast的参数需要halfExtents，因此，这里计算相机近平面一半，代码如下
```csharp
Vector3 CameraHalfExtends = Vector3.zero;
float halfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
CameraHalfExtends.y = halfHeight * camera.nearClipPlane;
CameraHalfExtends.x = CameraHalfExtends.y * camera.aspect;
```

### 4.2 BoxCast
在计算出CameraHalfExtends后，就可以调用Physics.BoxCast函数进行判断
```csharp
// 其中第二个参数CameraHalfExtends在文档内表示Half the size of the box in each dimension.
// 以下表示从focusPoint开始，向相机方向进行BoxCast，如果碰撞到了物体，就将focusPoint设置为碰撞点
if (Physics.BoxCast(focusPoint, CameraHalfExtends, -lookDirection, out RaycastHit hit, transform.rotation, distance))
{
    focusPoint = hit.point;
}
```