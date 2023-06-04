# 学习总结

## 1. 使用Unity物理来移动
Unity中使用的物理引擎为Nvidia的PhysX,可以给场景中的物体挂载RigidBody组件来使用物理引擎, RigidBody有两种控制运动的方式
* 第一种是非Kinematic的RigidBody,这种RigidBody可以通过设置速度和加速度来控制物体的运动,速度和加速度的计算公式如下:
  * 速度 = 速度 + 加速度 * 时间
  * 位移 = 速度 * 时间
  * 其中时间为Time.deltaTime，表示每一帧的时间，速度和位移为Vector3类型，表示三维空间中的速度和位移。
* 第二种是Kinematic的RigidBody,这种RigidBody可以通过设置位移来控制物体的运动,位移的计算公式如下:
  * 位移 = 位移 + 速度 * 时间
  * 其中时间为Time.deltaTime，表示每一帧的时间，速度和位移为Vector3类型，表示三维空间中的速度和位移。


## 2. 为物体添加跳跃功能
在实现跳跃功能之前，需要先了解下Unity内物理相关函数的执行顺序
* 首先执行FixedUpdate函数: 固定的物理更新时间，一般为0.02s，可以在Edit->Project Settings->Time中修改
* 然后执行OnTriggerXXX
* 然后执行OnCollisionXXX
* 最后是yield WaitForFixedUpdate()

Unity中检测跳跃可以使用Input.GetButtonDown("Jump")来实现

### 2.1 多重跳跃实现
可以使用一个变量jumpPhase来记录当前从地面开始跳跃的次数，然后定义一个跳跃的最大次数maxJumpPhase，当jumpPhase小于maxJumpPhase时，可以执行跳跃操作，否则不能执行跳跃操作

## 3. 地面检测存在多个碰撞体时
在地面检测时，如果地面有多个碰撞体，那么会多次触发OnCollisionStay函数，因此可以定义一个groundContactCount来记录每帧接触碰撞体的数量
注意：需要在FixedUpdate函数最后将groundContactCount置为0，然后在执行FixedUpdate之后的OnCollisionStay函数中将groundContactCount加1,这样就可以保证下一次FixedUpdate使用的是最新的groundContactCount