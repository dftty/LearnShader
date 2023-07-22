# 复习第五节问题

* 实现相机的up轴对齐物体时，计算重力对齐方式错误，原因是四元数的乘法不满足交换律，应该使用如下方式
```cs
Quaternion lookRotation = gravityAligment * orbitRotation
```