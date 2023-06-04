# 学习总结

本节学习了在Unity中获取用户输入，以及如何使用输入来控制游戏对象的移动。

* 获取输入的函数为Input.GetAxis(),返回值为float类型，范围为-1到1，可以通过修改Edit->Project Settings->Input来修改输入的按键。
* 在物理学中，速度公式如下 v = v0 + at，其中v0为初始速度，a为加速度，t为时间，Unity中的速度公式如下：
  * 速度 = 速度 + 加速度 * 时间
  * 位移 = 速度 * 时间
  * 其中时间为Time.deltaTime，表示每一帧的时间，速度和位移为Vector3类型，表示三维空间中的速度和位移。
  * 注意，获取输入以及计算最大速度可以放在Update函数中执行，而计算位移应该放在FixedUpdate函数中执行，因为Update函数的执行频率不固定，而FixedUpdate函数的执行频率固定