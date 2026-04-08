## 架构

### 三层结构

```
src/
├── Shell/LusterMotion/          # 宿主程序，Prism Bootstrapper 入口
├── Infrastructure/Luster.Prism/ # 基础设施：Prism 扩展、DI、配置、日志（Serilog）
└── Modules/                     # 42 个功能模块
```

### 模块分类

- **Common**: Assets（多语言/浮动窗口）、DataAccess（FreeSql+SQLite）、DataStruct、Tools、Authorization
- **Motion.TaskFlow.Engine**: 任务流引擎核心，包含 `IMotionController`、`IMotionEngine`
- **Motion.Business / Stations / Algorithm / Logic**: 业务逻辑、工位、算法、运动控制
- **Motion.CommonUI**: 公共 UI 组件（CommandModel、GlobalProperty、CommonBus）、ViewModel 基类
- **Motion.SubSystem**: 工具栏、登录、PLC 等子系统 UI
- **Motion.EditorUI**: 流程编辑器
- **Motion.DigitalSetup**: 数字架线（数据校验、点检、配置）
- **SimDevice**: 设备仿真层（相机、激光、灯光、运动卡等）

### Prism 模块化

模块在 `App.xaml.cs` 中通过 `ConfigureModuleCatalog` 注册，部分模块通过 `CommonBus.RegisterSystemDll()` 动态加载。每个模块实现 Prism `IModule` 接口。

### 核心服务

- **ICommonBus** (`CommonBus.cs`): 全局服务总线，负责配方管理、用户管理、事件发布、导航、数据映射
- **EventBus**: Prism EventAggregator，关键事件包括 `OperationEvent<StatusChanged>`（设备状态变更）、`RecipeOpenEvent`、`ProjectChangeEvent`
- **IMotionController**: 运动控制器，`Start()/Pause()/Stop()/Home()` 等操作入口
- **IMotionEngine**: 运动引擎，`EngineStatus` 枚举表示状态（Idle/Running/Stop/Pause/MaterialPending/Ready）
- **GlobalProperty**: 静态附加属性，`IsEnabeld` 控制界面全局启用/禁用，XAML 通过 `(g:GlobalProperty.IsEnabeld)` 绑定

### ViewModel 层次

```
MotionVM (基类，持有 commonBus、EventBus)
├── MotionPageVM (INavigationAware)
│   ├── DigitalAssContentVM
│   └── 各子页面 ViewModel
└── 其他业务 ViewModel
```

### 设备状态流转

启动命令 → `ToolBarContentVM` → `mController.Start()` → 引擎状态变更 → `OperationEvent` 发布 → 各模块订阅 `StatusChanged` 更新 UI。

### NuGet 包管理

使用中央包管理（`Directory.Packages.props`），私有源为 GitLab NuGet，配置在 `NuGet.config`。