# 复习第五节问题

* 实现相机的up轴对齐物体时，计算重力对齐方式错误，原因是四元数的乘法不满足交换律，应该使用如下方式
```cs
Quaternion lookRotation = gravityAligment * orbitRotation
```

# 复习第八节问题

* 当角色在爬墙时，跨过一个凸90度墙角，如果当前爬行加速度过小，而爬行速度又过大的话，会导致角色掉下来，而不是爬到另一个墙上


# 第四节问题

* 相机自动旋转时，rotationChange需要如下计算
```cs
float rotationChange =
			    rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
```

# 第七节问题

在计算相对平面速度connectionVelocity后，没有在ClearState函数内清除该数据