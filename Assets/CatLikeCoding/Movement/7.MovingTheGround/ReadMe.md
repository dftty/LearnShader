# 移动平面实现

创建移动物体，使用Unity的Acimation系统来创建移动平面，注意平面上需要添加刚体组件，并且将刚体的`Is Kinematic`属性勾选上，这样才能使得平面不受重力影响。
移动平面的Animator的UpdateMode需要设置为`Animate Physics`，这样才能使得平面的移动和刚体的移动同步。

## 旋转平面
当物体在旋转平面时，此时平面本身可能没有移动仅有旋转，因此这里记录接触点位置为物体的世界坐标，然后将其转换到旋转平面的局部坐标下