# 复杂重力效果

## 实现GravityPlane

在实现重力平面，然后组成一个box之后，为了解决移动物体在重力平面交界处平滑过渡的问题，需要进行如下的修改

* 物体的最大加速度maxAcceleration必须大于maxSpeed，测试后，maxSpeed = 10, maxAcceleration = 20，效果不错
* 物体的maxGroundAngle需要设置为45度以上
* 物体碰撞体的物理材质Friction Combine和Bounce Combine设置为Minimum

原因分析：
1. 经过测试，第一条中其实和maxAcceleration的大小有关系，根据MoveSphere中计算速度的方式，当物体撞向两个垂直的重力平面之间时，速度会在一瞬间变为0，这时候需要在当前这一帧加速度足够大，才能保证物体走出这个夹缝，否则就会一直卡住
2. 当物体处于夹缝中间时，此时物体的upAxis为夹缝的法线，此时和物体交互的两个重力平面和upAxis之间的夹角为45度，所以当maxGroundAngle设置小于45度时，此时就会认为物体不在地面上，导致物体无法前进
3. 和Friction Combine的设置有关系，当设置为Average时，物体在两个重力平面之间的摩擦力会变大，这是如果提高物体的maxAcceleration，也能够解决物体卡住的问题