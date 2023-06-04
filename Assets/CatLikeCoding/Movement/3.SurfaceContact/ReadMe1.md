# 记录自己重新实现一遍MovingSphere时，遇到的问题

## 1.加速度变化量计算错误
* 在计算出当前输入对应的最大速度desiredVelocity后，在FixedUpdate中计算当前速度到目标速度，调用Mathf.MoveTowards第三个参数仅传递了Time.deltaTime，导致速度变化量过小，导致速度增长非常慢，实际应该传递maxAccerlation * Time.deltaTime


## 2.根据碰撞体法线判断是否是地面
* 应该使用碰撞法线向量的y值和定义的maxGroundDotProduct做对比，当y大于maxGroundDotProduct时，表示碰撞体法线向量与y轴的夹角小于x度，即为地面


## 3.跳跃速度增量
当可以多次跳跃时，应该防止跳跃速度越来越大，因此每次起跳时应该检测当前物体在y轴上的速度，如果速度大于0，则应该取0或者跳跃速度减去y轴速度中的最大值

## 4.在脚本中定义了stairLayer，但是没有在prefab中选择Stairs层级
* 导致GetMinDot函数 一直返回地面角度点积