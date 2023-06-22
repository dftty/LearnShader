# 重新实现遇到的问题

## 物体层级错误
新增层级介绍
* Agent 玩家控制的角色所使用的层级
* Stairs 楼梯层级，与Detailed层级共同使用，用于在一个类似楼梯的模型上，该模型上会防止两个碰撞体，其中一个碰撞体的层级是Stairs，用于和玩家进行交互，在玩家检测到这个层级后，就可以使用特殊角度爬坡，另一个碰撞体的层级是Detailed，不与玩家进行交互，但是与其他物体进行交互，以便表现真实的楼梯效果
* Detailed 细节层级
在添加maxStairAngle以及stairMask以后，需要将物体的层级设置为Agent


## 相机跟随

### 定义focusRadis后遇到的问题
初始化时，相机会跟随物体，此时会有一个focusPoint，当物体移动后，物体位置与当前相机focusPoint之间的距离小于focusRadis时，相机不会跟随物体移动

当距离大于focusRadis后，会计算相机位置，计算方式如下：
```csharp
Vector3 targetPoint = focus.position;
float distance = Vector3.Distance(focusPoint, targetPoint);

if (distance > focusRadis)
{
    // 注意这里是从物体位置到相机当前跟随位置插值
    focusPoint = Vector3.Lerp(targetPoint, focusPoint, focusRadis / distance);
}
```

### 相机focusPoint与物体距离小于focusRadis的处理

当相机的focusPoint与物体距离小于focusRadis时，我们希望相机以更加平滑的方式向中心移动，因此这里我们提出半衰期的概念，当距离小于focusRadis时，我们让相机每1s缩小一半的距离，直到距离小于一个足够小的值0.001. 
此时每隔一秒距离的计算公式就是 y = (1 / 2) ^ x，但是在游戏内，一秒的时间会走过许多帧，因此我们需要将这个公式转换为每帧的计算公式，即 y = (1 / 2) ^ (x / 60)，其中60是每秒的帧数，这样就可以在每帧计算出相机的focusPoint，从而实现相机的平滑移动

```csharp
[Range(0, 1)]
focusCentering = 0.75f;

float t = 1;
if (distance > 0.001f && focusCentering > 0)
{
    t = Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime);
}
```


### 相机角度计算

```csharp
orbitAngle += rotateSpeed * input * Time.unscaledDeltaTime;
```

### 三维向量旋转

对于一个四元数的角度quaternion，如果一个向量想要旋转到该角度，可以使用如下方式
```csharp
// 注意： 四元数必须放在vector3的前面
Vector3 direction = quaternion * Vector3.forward;
```

### 角色朝着相机朝向前进

让角色脚本持有相机Transform，然后获取相机的forward和right向量，让角色朝着该向量前进即可

```csharp
// 因为角色移动的平面是XZ平面，因此将向量的y都设置为0
Vector3 forward = cameraTransform.forward;
forward.y = 0;
forward.Normalize();
Vector3 right = cameraTransform.right;
right.y = 0;
right.Normalize();

// 根据forward以及right计算速度
```