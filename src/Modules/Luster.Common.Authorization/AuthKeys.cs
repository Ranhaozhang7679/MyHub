using System;
using System.Collections.Generic;

namespace DC.Authorization
{
    /// <summary>
    /// 权限点结构体，具备 Module、View、Operation、Order 四个属性。
    /// Order 用于控制同一分组下的排序顺序。
    /// </summary>
    public struct AuthItem
    {
        public string Module { get; set; }
        public string View { get; set; }
        public string Operation { get; set; }
        public string Description { get; set; }
        /// <summary>排序序号，数值越小越靠前</summary>
        public int Order { get; set; }

        public AuthItem(string module, string view, string operation, string description = "", int order = 0)
        {
            Module = module;
            View = view;
            Operation = operation;
            Description = description;
            Order = order;
        }

        public override string ToString() => Operation;
    }

    /// <summary>
    /// 全局权限定义类。统一定义所有的 AuthItem 常量。
    /// ViewModel、配置特性和 XAML 均可直接引用这里的 AuthItem。
    /// </summary>
    public static class AuthDictionary
    {
        // ===============================================
        // 操 作 权 限
        // ===============================================
        public static readonly AuthItem ModifyRight = new AuthItem("基础模块", "权限设置", "修改权限", "允许修改系统基础权限设置");


        // ── 主页工具栏 ──
        public static readonly AuthItem VizModeSwitch = new AuthItem("主页", "工具栏", "运行模式切换", "工具栏模式切换下拉按钮",1);
        public static readonly AuthItem VizPageHome = new AuthItem("主页", "工具栏", "主页", "",2);
        public static readonly AuthItem VizPageHardWare = new AuthItem("主页", "工具栏", "硬件", "",3);
        public static readonly AuthItem VizPageFlow = new AuthItem("主页", "工具栏", "流程", "",4);
        public static readonly AuthItem VizPageAlarm = new AuthItem("主页", "工具栏", "报警", "",5);
        public static readonly AuthItem VizPageStatistics = new AuthItem("主页", "工具栏", "统计", "",6);
        public static readonly AuthItem VizPageConfigure = new AuthItem("主页", "工具栏", "配置", "",7);
        public static readonly AuthItem VizPageIntegratedHardware = new AuthItem("主页", "工具栏", "软硬件调试", "",8);
        public static readonly AuthItem VizPageProject = new AuthItem("主页", "工具栏", "工程", "",9);
        public static readonly AuthItem VizPageLightTuning = new AuthItem("主页", "工具栏", "光调", "",15);
        public static readonly AuthItem VizCmdStart = new AuthItem("主页", "工具栏", "启动", "",10);
        public static readonly AuthItem VizCmdReset = new AuthItem("主页", "工具栏", "复位", "",11);
        public static readonly AuthItem VizCmdPause = new AuthItem("主页", "工具栏", "暂停", "",12);
        public static readonly AuthItem VizCmdStop = new AuthItem("主页", "工具栏", "停止", "",13);
        public static readonly AuthItem VizCmdHomeZero = new AuthItem("主页", "工具栏", "回零", "",14);


        // ── 硬件页面 - 管理 ──
        public static readonly AuthItem DevAlarmConfig = new AuthItem("硬件", "设备管理", "报警配置", "报警配置管理页面", 1);
        public static readonly AuthItem DevMaintain = new AuthItem("硬件", "设备管理", "设备保养", "设备保养管理页面", 2);
        public static readonly AuthItem DevModuleName = new AuthItem("硬件", "设备管理", "模组名称", "模组名称配置页面", 3);
        public static readonly AuthItem DevAxisIODebug = new AuthItem("硬件", "设备管理", "轴点位调试", "轴IO调试页面", 4);
        public static readonly AuthItem DevModuleConfig = new AuthItem("硬件", "设备管理", "模块配置", "虚拟模组配置页面", 5);
        public static readonly AuthItem DevAlarmCustom = new AuthItem("硬件", "设备管理", "自定义报警配置", "自定义报警配置页面", 6);
        public static readonly AuthItem DevPositionParameter = new AuthItem("硬件", "设备管理", "点位参数配置", "位置参数配置页面", 7);
        public static readonly AuthItem DevKeyParameter = new AuthItem("硬件", "设备管理", "关键参数配置", "关键参数配置页面", 8);

        // ── 硬件页面 - 真实设备 ──
        public static readonly AuthItem DevCamera = new AuthItem("硬件", "硬件设备", "相机", "2D相机设备管理", 10);
        public static readonly AuthItem DevLineLaser = new AuthItem("硬件", "硬件设备", "线激光", "3D线激光设备管理", 11);
        public static readonly AuthItem DevMotionCard = new AuthItem("硬件", "硬件设备", "运动控制卡", "运动控制卡设备管理", 12);
        public static readonly AuthItem DevLightController = new AuthItem("硬件", "硬件设备", "光源控制器", "光源控制器设备管理", 13);
        public static readonly AuthItem DevRobot = new AuthItem("硬件", "硬件设备", "机器人", "机器人设备管理", 14);
        public static readonly AuthItem DevPrinter = new AuthItem("硬件", "硬件设备", "打印机", "打印机设备管理", 15);
        public static readonly AuthItem DevFXTCP = new AuthItem("硬件", "硬件设备", "FXTCP", "FXTCP设备管理", 16);

        // ── 硬件页面 - 虚拟设备 ──
        public static readonly AuthItem DevVAxis = new AuthItem("硬件", "虚拟设备", "虚拟轴", "虚拟轴设备管理", 20);
        public static readonly AuthItem DevVIO = new AuthItem("硬件", "虚拟设备", "虚拟IO", "虚拟IO信号管理", 21);
        public static readonly AuthItem DevVLineLaser = new AuthItem("硬件", "虚拟设备", "虚拟线激光", "虚拟3D激光管理", 22);
        public static readonly AuthItem DevVCamera = new AuthItem("硬件", "虚拟设备", "虚拟相机", "虚拟2D相机管理", 23);
        public static readonly AuthItem DevVVacuum = new AuthItem("硬件", "虚拟设备", "虚拟真空", "虚拟真空设备管理", 24);
        public static readonly AuthItem DevVCylinder = new AuthItem("硬件", "虚拟设备", "虚拟气缸", "虚拟气缸设备管理", 25);
        public static readonly AuthItem DevVAxisM = new AuthItem("硬件", "虚拟设备", "虚拟多轴", "虚拟多轴设备管理", 26);
        public static readonly AuthItem DevVPrinter = new AuthItem("硬件", "虚拟设备", "虚拟打印机", "虚拟打印机设备管理", 27);
        public static readonly AuthItem DevVPlc = new AuthItem("硬件", "虚拟设备", "虚拟PLC", "虚拟PLC设备管理", 28);
        public static readonly AuthItem DevVIOSimulation = new AuthItem("硬件", "虚拟设备", "虚拟IO仿真", "虚拟串口IO仿真", 29);
        public static readonly AuthItem DevVCommunication = new AuthItem("硬件", "虚拟设备", "虚拟通信", "虚拟通用通信设备", 30);
        public static readonly AuthItem DevVPCylinder = new AuthItem("硬件", "虚拟设备", "虚拟P气缸", "虚拟P气缸设备管理", 31);
        public static readonly AuthItem DevVRobot = new AuthItem("硬件", "虚拟设备", "虚拟机器人", "虚拟机器人设备管理", 32);
        public static readonly AuthItem DevVFlyingPhoto = new AuthItem("硬件", "虚拟设备", "虚拟飞拍", "虚拟飞拍模块管理", 33);
        public static readonly AuthItem DevVModule = new AuthItem("硬件", "虚拟设备", "虚拟模组", "虚拟模组设备管理", 34);


        // ── 流程页面 - 左侧面板 ──
        public static readonly AuthItem FlowFuncModule = new AuthItem("流程", "左侧面板", "功能模块", "功能模块页面", 1);
        public static readonly AuthItem FlowParamConfig = new AuthItem("流程", "左侧面板", "参数配置", "参数配置页面", 2);
        public static readonly AuthItem FlowSearchModule = new AuthItem("流程", "左侧面板", "搜索模块", "搜索模块页面", 3);

        // ── 流程页面 - 编辑器 ──
        public static readonly AuthItem FlowEditor = new AuthItem("流程", "编辑器", "流程编辑器", "流程编辑器页面", 10);

        // ── 流程页面 - 右侧面板 ──
        public static readonly AuthItem FlowLog = new AuthItem("流程", "右侧面板", "日志", "日志面板", 20);
        public static readonly AuthItem FlowGlobalVar = new AuthItem("流程", "右侧面板", "全局变量", "全局变量面板", 21);
        public static readonly AuthItem FlowAxisDebug = new AuthItem("流程", "右侧面板", "轴调试", "轴调试面板", 22);
        public static readonly AuthItem FlowCT = new AuthItem("流程", "右侧面板", "单片耗时", "单片耗时面板", 23);
        public static readonly AuthItem FlowCacheData = new AuthItem("流程", "右侧面板", "缓存数据", "缓存数据面板", 24);
        public static readonly AuthItem FlowStationOverview = new AuthItem("流程", "右侧面板", "工站总览", "工站总览面板", 25);

        // ── 配置页面 - 右侧列表 ──
        public static readonly AuthItem CfgMachineConfigure = new AuthItem("配置", "配置页面", "机器配置", "机器参数配置页面", 1);
        public static readonly AuthItem CfgPLCConfigure = new AuthItem("配置", "配置页面", "PLC配置", "PLC报警配置页面", 2);
        public static readonly AuthItem CfgSoftConfigure = new AuthItem("配置", "配置页面", "软件配置", "软件用户配置页面", 3);
        public static readonly AuthItem CfgCockpit = new AuthItem("配置", "配置页面", "驾驶舱", "产品信息驾驶舱页面", 4);
        public static readonly AuthItem CfgRobotInfo = new AuthItem("配置", "配置页面", "机器人信息", "机器人配置页面", 5);
        public static readonly AuthItem CfgFileConfig = new AuthItem("配置", "配置页面", "文件配置", "文件管理配置页面", 6);
        public static readonly AuthItem CfgVisionInfo = new AuthItem("配置", "配置页面", "视觉信息", "视觉配置页面", 7);
        public static readonly AuthItem CfgFXTCP = new AuthItem("配置", "配置页面", "FXTCP", "FXTCP配置页面", 8);
        public static readonly AuthItem CfgFunctionEnable = new AuthItem("配置", "配置页面", "功能使能", "功能使能配置页面", 9);


    }
}
