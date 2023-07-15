# 复习第五节问题

* 添加自定义重力后，没有将物体刚体组件的useGravity设置为false
* 相机跟随脚本中gravityAligment没有初始化为Quaternion.identity
* 物体脚本中跳跃函数在添加gravity参数后，计算跳跃速度处没有将-2修改为2，导致计算的跳跃速度为NaN
* SnapToGround函数，忘记检测碰撞体是否满足地面条件
* 物体脚本中需要将计算点积处替换为upAxis
  * EvaluteCollision函数
  * CheckSteepContact函数
  * SnapToGround函数
  

# 复习第六节问题

* 实现重力盒外部重力时，如果盒子本身有旋转，那么计算出物体和盒子的相对位置之后，需要调用tranform.TransformDirection函数将相对位置转换为局部坐标系下的位置
* 重力计算完成之后，还需要转换回去

# 复习第七节问题

* 在EvaluateCollision中，应该优先使用地面连接物体作为connectedBody，如果没有地面连接物体，再使用其他连接物体
* UpdateConnectionVelocity函数中，记录物体位置和物体相对于移动平面的位置。