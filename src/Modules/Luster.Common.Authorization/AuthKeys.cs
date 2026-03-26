using System;
using System.Collections.Generic;

namespace DC.Authorization
{
    /// <summary>
    /// 权限点结构体，具备 Module、View、Operation 三个属性。
    /// </summary>
    public struct AuthItem
    {
        public string Module { get; set; }
        public string View { get; set; }
        public string Operation { get; set; }
        public string Description { get; set; }

        public AuthItem(string module, string view, string operation, string description = "")
        {
            Module = module;
            View = view;
            Operation = operation;
            Description = description;
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
        public static readonly AuthItem ModifyRight = new AuthItem("基础模块", "系统设置", "修改权限", "允许修改系统基础权限设置");
        public static readonly AuthItem ModifyPassword = new AuthItem("基础模块", "账户管理", "修改密码", "");
        public static readonly AuthItem ChangeMotorSpeed = new AuthItem("产线模块", "高级参数页", "修改电机速度", "具有更改电机速度的危险权限");

        // ===============================================
        // 可 见 性 管 控
        // ===============================================
        public static readonly AuthItem VizAdvancedConfigTab = new AuthItem("基础模块", "高级参数页", "显示高级参数页", "控制主界面能否看见该分页");
        public static readonly AuthItem VizBypassStationSwitch = new AuthItem("产线模块", "工艺流程", "显示跳过工站开关", "显示跳过工站的开关");
    }
}
