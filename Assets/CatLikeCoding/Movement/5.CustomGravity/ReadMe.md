# 实现自定义的重力方向

## 1. 重力方向的定义

在脚本中定义upAxis，表示物体当前重力的反方向，接下来需要修改所有和重力相关的代码，将原来的Vector3.up修改为upAxis，以及根据重力角度计算点积的代码

注意：在定义upAxis后，需要将物体prefab的RigidBody组件中的useGravity设置为false

## 2. 定义物体的移动轴forwardAxis和rightAxis

这两个移动轴需要在Update函数内投影到重力方向对应的平面上，然后在移动方法内使用这两个移动轴代替之前的Vector3.forward以及Vector3.right

## 3. 简单的自定义重力

这里我们将重力源定为世界原点，那么物体位置向量就是重力的方向

### 3.1 在相机脚本中遇到的问题
在lateUpdate函数中计算gravityAlignment时，第二个参数应该是CustomGravity1.GetUpAxis(focusPoint)