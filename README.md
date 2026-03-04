# HachiFramework

基于 Unity 2022 + URP 的轻量级组件化游戏框架，面向独立单机游戏的快速原型开发
## 架构特点

- **两阶段启动**: Launch 场景负责资源系统初始化，Main 场景负责框架和业务启动，职责分离
- **组件化管理器**: GameEntry 自动收集子物体上所有 `IGameManager`，统一驱动生命周期和帧循环，新增 Manager 只需挂组件
- **零反射 MVC UI**: UIConfig 通过泛型工厂委托创建 Ctrl/View/Model，编译期类型安全
- **泛型消息总线**: `MessageBroker<T>` 利用 C# 泛型静态字段按类型隔离，无需 Dictionary 查找
- **状态机流程管理**: Inspector 可视化配置流程节点和入口，支持黑板数据

## 模块介绍

### GameEntry — 组件化管理器中心
饿汉式单例，自动收集并注册所有 Manager，统一驱动 Update/LateUpdate/FixedUpdate。支持运行时动态注册和注销，通过 `Get<T>()` 泛型获取任意 Manager。

### AssetManager — 资源管理
封装 YooAsset，提供同步/异步资源加载，内部维护 Handle 引用计数自动管理资源释放。支持场景异步加载卸载，路径自动补全前缀。

### UIManager — MVC UI 系统
7 层分层 Canvas 架构（Hud → Loading），面板完整生命周期（Loading → Show ↔ Hide → 销毁），First/Second 层栈管理。配套编辑器工具一键生成 MVC 代码并自动注册。自带屏幕适配。

### ProcedureManager — 流程管理
基于有限状态机驱动游戏流程切换，Inspector 中勾选可用流程并指定入口节点。ProcedureBase 提供 OnEnter/OnExit/OnUpdate 生命周期和 ChangeState 切换能力。

### MessageManager — 消息总线
泛型静态单例 `MessageBroker<T>.Default`，侵入式双向链表存储订阅者，线程安全。支持 Action 匿名订阅和自定义 Handler 两种方式，Subscribe 返回 IDisposable 方便取消。

### GamePoolManager — 对象池
Rent/Return 模式，API 参数与 Object.Instantiate 对齐，支持 GameObject 和 Component 泛型租借。提供 Prewarm 预热和 IPoolCallbackReceiver 池回调通知。

### InputManager — 输入管理
封装 Unity Input System 生成的 GameInputs，暴露 Movement、CameraLook、Run 等属性，通过 IGameManager 生命周期管理输入的 Enable/Disable。

### 3C 系统（业务层）
第三人称相机控制（输入驱动旋转 + SmoothDamp 平滑 + 距离跟随）、相机防穿墙碰撞检测、角色移动基类（重力模拟 + 地面检测 + 坡道投影 + Root Motion）、玩家控制器（输入→相机相对旋转→Animator 驱动移动）。
