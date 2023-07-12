# 复习第五节问题

* 添加自定义重力后，没有将物体刚体组件的useGravity设置为false
* 相机跟随脚本中gravityAligment没有初始化为Quaternion.identity
* 物体脚本中跳跃函数在添加gravity参数后，计算跳跃速度处没有将-2修改为2，导致计算的跳跃速度为NaN
* SnapToGround函数，忘记检测碰撞体是否满足地面条件
* 物体脚本中需要将计算点积处替换为upAxis
  * EvaluteCollision函数
  * CheckSteepContact函数
  * SnapToGround函数