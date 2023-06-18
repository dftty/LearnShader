# 重写遇到的bug

## 跳跃函数在一次按下后执行了两次
原因：在打开prefab将脚本拖到物体上之后，没有注意到因为unity的bug导致脚本被加了两个

## ClearState中状态赋值错误
contactNormal为物体地面法线向量，因此在ClearState函数中应该将其修改为Vector3.zero，这样在FixedUpdate函数执行之后，执行的OnCollisionXXX函数就可以正确计算出地面法线了，
注意，这样计算好的地面法线是所有接触面为地面的法线叠加和。因此在下一帧进入FixedUpdate之后，需要在UpdateState中进行判断，如果在地面上，那么contactNormal需要归一化

## SnapToGround
该函数具体实现请看3.SurfaceContact中的Readme1。因为进入了该函数，表示上一帧OnCollisionXXX计算中地面接触数量和法线分别是0，和zero，那么这里如果检测到了地面，就需要将结束数量和法线设置为1和碰撞体法线

然后计算物体当前速度投影到地面法线后，如果该值大于0时，才表示有跳起的趋势，这种情况下才需要重新计算速度

添加了stepSinceLastJump参数后，在判断是否需要贴地时，需要判断stepSinceLastJump是否小于等于2


## 计算速度错误
计算出移动方向在当前法线对应平面的投影后，没有把当前物体速度投影到计算出来的移动方向上
```csharp
float currentX = Vector3.Dot(velocity, xAxis);
float currentZ = Vector3.Dot(velocity, zAxis);
```

## 学习过程中注意到的一个关于向量归一化的问题
```csharp
Vector2 a = new Vector2(1, 0.3f);
Debug.Log(a);                       //在Unity中会输出 (1.0, 0.3)
a.Normalize();
Debug.Log(a.magnitude);             //在Unity中会输出 1
Debug.Log(a.sqrMagnitude);          //在Unity中会输出 1
Debug.Log(a.ToString());            //在Unity中会输出 (1.0, 0.3)  这里看起来像没有归一化，但是其实已经归一化了，原因是ToString()函数对x和y进行了四舍五入
Debug.Log(a.x);                     //在Unity中会输出 0.9578263
Debug.Log(a.y);                     //在Unity中会输出 0.2873479
```


## 实现相机根据输入旋转出现的问题

在添加旋转之前，相机看向物体的角度是固定的
```csharp
Vector3 direction = transform.forward;
```

添加旋转功能之后，相机看向物体的角度会随着旋转而变化
```csharp
// 这里四元数乘以向量表示将向量旋转到四元数对应的角度
Vector3 direction = Quaternion.Euler(orbitAngles) * Vector3.forward;
```