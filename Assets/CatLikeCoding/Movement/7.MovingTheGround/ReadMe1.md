# 重新实现第三节

## 墙跳问题
在重新实现第三节的脚本之后，发现在墙跳场景中跳跃高度和教程中有出入，首先第一个应该设置跳跃高度为3，接下来是碰撞体物理材质都设置为0和Average

# 重新实现第四节

## 相机跟随
插值方式采用的是从当前物体位置到上一次相机记录的跟随位置

## 相机旋转
对于看着场景中物体的相机，上下旋转相当于x轴，左右旋转相当于y轴，因此在获取鼠标输入时，x输入应该对应Vertical Camera，y输入应该对应Horizontal Camera

相机自动旋转的目标是根据物体相对于上一帧的位置所计算出来的向量

在加入相机旋转之后，此时在相机跟随处计算位置的代码需要进行如下处理，如果不修改，那么会导致相机抖动
```csharp
Quaternion lookRotation = Quaternion.Euler(orbirAngles);

Vector3 lookDirection = lookRotation * Vector3.forward;
// 旧代码
// 这里之前使用的是transform.forward，因为添加了旋转功能，所以这里需要使用lookDirection
// Vector3 lookPosition = focusPoint - transform.forward * distance;
Vector3 lookPosition = focusPoint - lookDirection * distance;
```

## 相机碰撞检测

在检测到碰撞后，相机位置的计算方式如下
```csharp
Vector3 rectPosition = castFrom + castDirection * hit.distance;
lookPosition = rectPosition - rectOffset;
```

# 重新实现第五节

* 定义CustomGravity脚本，其中有获取重力以及获取UpAxis的静态函数，获取重力函数简化为当前物体位置乘以物理世界的重力的y分量
* 在MovingSphere中的FixedUpdate获取gravity后，此脚本需要禁用刚体的重力，然后将获取的重力应用到速度上
* MovingShpere中根据法线的y分量计算地面角度的代码也需要相应修改
* 物体脚本添加自定义重力后，相机跟随的角度也需要相应旋转
* 相机的自动旋转函数内的位移计算需要进行反重力方向对齐


# 重新实现第六节

## 重力平面
有一个实现的细节在第六节没有注意，重力平面脚本挂载的物体在其显示位置向上1个单位处
在CustomGravity脚本的获取重力函数中，最后返回值错误的进行了归一化操作，导致物体下落速度变得非常慢

重力平面重力计算方式：物体处在重力平面上的任意位置，重力方向应该都是垂直于该平面，然后随着物体距离平面减小，因此首先计算物体到平面的距离，计算方式如下：
```csharp
float distance = Vector3.Dot(transform.up, position - transform.position);
```

物体在上下两个重力平面之间相互切换时，相机的旋转需要平滑过渡，而不是瞬间切换

## 重力球
重力球计算重力使用的是重力球位置减去物体位置的向量，相当于物体指向重力球的向量