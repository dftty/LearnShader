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