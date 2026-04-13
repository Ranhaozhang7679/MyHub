using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class SubTypeMapping
    {
        public string ErrorType { get; set; }
        public List<ComboOption> Options { get; set; } = new List<ComboOption>();
    }

    public class AlarmWizardConfigModel
    {
        public List<ComboOption> ErrorTypes { get; set; } = new List<ComboOption>();
        public List<SubTypeMapping> ErrorSubTypes { get; set; } = new List<SubTypeMapping>();
        public List<ComboOption> Components { get; set; } = new List<ComboOption>();
        public List<ComboOption> SubComponents { get; set; } = new List<ComboOption>();
        public List<ComboOption> RepairActions { get; set; } = new List<ComboOption>();

        public static AlarmWizardConfigModel GetDefaultConfig()
        {
            var config = new AlarmWizardConfigModel();
            
            // 报警类型
            config.ErrorTypes.Add(new ComboOption { Value = "M", Header = "Motion Error (M) - 运动报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "C", Header = "Pneumatic Cylinder Error (C) - 气缸报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "U", Header = "Vacuum Error (U) - 真空报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "S", Header = "Sensor Error (S) - 传感器报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "L", Header = "Laser Error (L) - 激光报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "W", Header = "Screwing Error (W) - 锁螺丝报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "N", Header = "Software Related Error (N) - 软件及通讯报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "V", Header = "Vision Error (V) - 视觉系统报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "T", Header = "Scanning Error (T) - 扫码及条码报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "F", Header = "Safety Error (F) - 安全门/急停设备报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "E", Header = "Material Shortage Error (E) - 缺料报警" });
            config.ErrorTypes.Add(new ComboOption { Value = "O", Header = "Other (O) - 其他报警" });

            // 报警子类
            // M
            var motionSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Motor/Robot Overload - 电机/机器人过载" },
                new ComboOption { Value = "02", Header = "02 - Not in position - 未到位" },
                new ComboOption { Value = "03", Header = "03 - Timeout - 运动超时" },
                new ComboOption { Value = "04", Header = "04 - Negative Position - 负向超限" },
                new ComboOption { Value = "05", Header = "05 - Positive Position - 正向超限" },
                new ComboOption { Value = "06", Header = "06 - Motor Enable Error - 电机使能错误" },
                new ComboOption { Value = "07", Header = "07 - Communication Error - 通讯异常" },
                new ComboOption { Value = "08", Header = "08 - Max Attempts reached - 重试次数超限" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "M", Options = motionSubs });

            // C
            var pneumaticSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Negative Position - 负位/退回到位" },
                new ComboOption { Value = "02", Header = "02 - Positive Position - 正位/伸出到位" },
                new ComboOption { Value = "03", Header = "03 - Communication Error - 通讯异常" },
                new ComboOption { Value = "04", Header = "04 - Air pressure/Air flow error - 气压/气流异常" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "C", Options = pneumaticSubs });

            // U
            var vacuumSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Vacuum Alarm - 真空度异常/吸料失败" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "U", Options = vacuumSubs });

            // S
            var sensorSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Load Cell force abnormal - 测力异常" },
                new ComboOption { Value = "02", Header = "02 - Sensor detected/not detected - 传感器检测异常" },
                new ComboOption { Value = "03", Header = "03 - Temperature alarm - 温度报警" },
                new ComboOption { Value = "04", Header = "04 - Communication Error - 通讯异常" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "S", Options = sensorSubs });

            // L
            var laserSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Laser not enabled - 激光未就绪" },
                new ComboOption { Value = "02", Header = "02 - Abnormal data - 数据异常" },
                new ComboOption { Value = "03", Header = "03 - Communication Error - 通讯异常" },
                new ComboOption { Value = "04", Header = "04 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "L", Options = laserSubs });
            
            // W
            var screwingSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Torque Alarm - 扭矩报警" },
                new ComboOption { Value = "02", Header = "02 - Cycle Alarm - 周期报警" },
                new ComboOption { Value = "03", Header = "03 - Communication Error - 通讯异常" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "W", Options = screwingSubs });

            // N
            var softwareSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Value out of range - 数值超限" },
                new ComboOption { Value = "02", Header = "02 - Data save failure - 数据保存失败" },
                new ComboOption { Value = "03", Header = "03 - Communication Error - 通讯异常" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "N", Options = softwareSubs });

            // V
            var visionSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Image Acquisition - 采图异常" },
                new ComboOption { Value = "02", Header = "02 - Image Processing - 处理异常" },
                new ComboOption { Value = "03", Header = "03 - Foolproofing - 防呆报警" },
                new ComboOption { Value = "04", Header = "04 - Inspection Tolerance - 公差超限" },
                new ComboOption { Value = "05", Header = "05 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "V", Options = visionSubs });

            // T
            var scanningSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Barcode Scan Error - 扫码失败" },
                new ComboOption { Value = "02", Header = "02 - Type Error - 类型错误/混料" },
                new ComboOption { Value = "03", Header = "03 - Improper Serial Number length - 条码长度异常" },
                new ComboOption { Value = "04", Header = "04 - Material Match Error - 物料匹配错误" },
                new ComboOption { Value = "05", Header = "05 - Process Error - 工序异常" },
                new ComboOption { Value = "06", Header = "06 - Communication Error - 通讯异常" },
                new ComboOption { Value = "07", Header = "07 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "T", Options = scanningSubs });

            // F
            var safetySubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - E-Stop Pressed - 急停按下" },
                new ComboOption { Value = "02", Header = "02 - Light Curtain/Door alarm - 光栅/安全门触发" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "F", Options = safetySubs });

            // E
            var materialSubs = new List<ComboOption>
            {
                new ComboOption { Value = "01", Header = "01 - Out of Material - 缺料" },
                new ComboOption { Value = "02", Header = "02 - Almost out of Material warning - 预缺料警告" },
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "E", Options = materialSubs });
            
            // O
            var otherSubs = new List<ComboOption>
            {
                new ComboOption { Value = "99", Header = "99 - Other - 其他" }
            };
            config.ErrorSubTypes.Add(new SubTypeMapping { ErrorType = "O", Options = otherSubs });

            // --- Components ---
            config.Components.Add(new ComboOption { Value = "CV", Header = "Conveyor (CV) - 传送带" });
            config.Components.Add(new ComboOption { Value = "DS", Header = "Dispense (DS) - 点胶阀" });
            config.Components.Add(new ComboOption { Value = "EE", Header = "Electric (EE) - 电气组件" });
            config.Components.Add(new ComboOption { Value = "EF", Header = "End Effector (EF) - 末端执行器" });
            config.Components.Add(new ComboOption { Value = "ES", Header = "Emergency Buttons (ES) - 急停按钮" });
            config.Components.Add(new ComboOption { Value = "GA", Header = "Gantry (GA) - 龙门模组" });
            config.Components.Add(new ComboOption { Value = "IP", Header = "IPC/Computer (IP) - 工控机" });
            config.Components.Add(new ComboOption { Value = "LC", Header = "Load Cell (LC) - 测力传感器" });
            config.Components.Add(new ComboOption { Value = "LO", Header = "Loader (LO) - 上料机" });
            config.Components.Add(new ComboOption { Value = "PL", Header = "PLC (PL) - PLC控制器" });
            config.Components.Add(new ComboOption { Value = "PN", Header = "Pneumatics (PN) - 气动组件" });
            config.Components.Add(new ComboOption { Value = "PS", Header = "Press (PS) - 压机组件" });
            config.Components.Add(new ComboOption { Value = "RB", Header = "Robot (RB) - 机器人" });
            config.Components.Add(new ComboOption { Value = "SC", Header = "Safety interlock/door/barrier (SC) - 安全锁/安全门" });
            config.Components.Add(new ComboOption { Value = "SS", Header = "Sensor/Signal (SS) - 传感器/信号" });
            config.Components.Add(new ComboOption { Value = "UL", Header = "Unloader (UL) - 下料机" });
            config.Components.Add(new ComboOption { Value = "VG", Header = "Vacuum generator (VG) - 真空发生器" });
            config.Components.Add(new ComboOption { Value = "VS", Header = "CCD or other vision system (VS) - 视觉系统" });
            config.Components.Add(new ComboOption { Value = "OO", Header = "Other (OO) - 其他" });

            // --- Sub-Components ---
            config.SubComponents.Add(new ComboOption { Value = "BL", Header = "Belts (BL) - 皮带" });
            config.SubComponents.Add(new ComboOption { Value = "A", Header = "Axis (A*) - 轴" });
            config.SubComponents.Add(new ComboOption { Value = "BO", Header = "Bolt (BO) - 螺栓" });
            config.SubComponents.Add(new ComboOption { Value = "BR", Header = "Bracket (BR) - 支架" });
            config.SubComponents.Add(new ComboOption { Value = "CA", Header = "Cables (CA) - 线缆" });
            config.SubComponents.Add(new ComboOption { Value = "CE", Header = "Connector (Electrical) (CE) - 电气接头" });
            config.SubComponents.Add(new ComboOption { Value = "CL", Header = "Clamp (CL) - 夹爪" });
            config.SubComponents.Add(new ComboOption { Value = "CO", Header = "Coupler (CO) - 联轴器" });
            config.SubComponents.Add(new ComboOption { Value = "CP", Header = "Connector (Pneumatic) (CP) - 气动接头" });
            config.SubComponents.Add(new ComboOption { Value = "CY", Header = "Cylinder (CY) - 气缸" });
            config.SubComponents.Add(new ComboOption { Value = "DE", Header = "Driver (Electrical) (DE) - 电气驱动器" });
            config.SubComponents.Add(new ComboOption { Value = "DR", Header = "Driver (Robot) (DR) - 机器人驱动器" });
            config.SubComponents.Add(new ComboOption { Value = "DV", Header = "Driver (Vision) (DV) - 视觉驱动器" });
            config.SubComponents.Add(new ComboOption { Value = "H", Header = "Holder/Nest (General) (H*) - 载具/底座" });
            config.SubComponents.Add(new ComboOption { Value = "ME", Header = "Mechanism (ME) - 机构" });
            config.SubComponents.Add(new ComboOption { Value = "M", Header = "Motor (M*) - 电机" });
            config.SubComponents.Add(new ComboOption { Value = "MP", Header = "Mechanical Part (MP) - 机械零件" });
            config.SubComponents.Add(new ComboOption { Value = "NE", Header = "Needle (NE) - 点胶针头" });
            config.SubComponents.Add(new ComboOption { Value = "NO", Header = "Nozzle (NO) - 吸嘴" });
            config.SubComponents.Add(new ComboOption { Value = "PC", Header = "PLC cable (PC) - PLC线缆" });
            config.SubComponents.Add(new ComboOption { Value = "PO", Header = "PLC connector (PO) - PLC接头" });
            config.SubComponents.Add(new ComboOption { Value = "PR", Header = "PLC card (PR) - PLC模块/板卡" });
            config.SubComponents.Add(new ComboOption { Value = "RO", Header = "Rollers (RO) - 滚轮" });
            config.SubComponents.Add(new ComboOption { Value = "RT", Header = "Robot Tool (RT) - 机器人工具" });
            config.SubComponents.Add(new ComboOption { Value = "SC", Header = "Screws (SC) - 螺丝" });
            config.SubComponents.Add(new ComboOption { Value = "SE", Header = "Sensor (Electrical) (SE) - 电气传感器" });
            config.SubComponents.Add(new ComboOption { Value = "SF", Header = "Sensor Flow (SF) - 流量传感器" });
            config.SubComponents.Add(new ComboOption { Value = "SH", Header = "Shim (SH) - 垫片" });
            config.SubComponents.Add(new ComboOption { Value = "SL", Header = "Sensor (Load-cell) (SL) - 称重/测力传感器" });
            config.SubComponents.Add(new ComboOption { Value = "SO", Header = "Stopper (SO) - 阻挡气缸" });
            config.SubComponents.Add(new ComboOption { Value = "SP", Header = "Sensor (Positioning) (SP) - 定位传感器" });
            config.SubComponents.Add(new ComboOption { Value = "SR", Header = "Sensor (Pressure) (SR) - 压力传感器" });
            config.SubComponents.Add(new ComboOption { Value = "TU", Header = "Tubes (TU) - 气管" });
            config.SubComponents.Add(new ComboOption { Value = "VD", Header = "Valve directional (VD) - 换向阀" });
            config.SubComponents.Add(new ComboOption { Value = "VP", Header = "Valve pressure (VP) - 调压阀" });
            config.SubComponents.Add(new ComboOption { Value = "VV", Header = "Valve vacuum (VV) - 真空阀/电磁阀" });
            config.SubComponents.Add(new ComboOption { Value = "PL", Header = "Peeler (PL) - 剥料器" });
            config.SubComponents.Add(new ComboOption { Value = "OO", Header = "Other (OO) - 其他" });

            // --- Repair Actions ---
            config.RepairActions.Add(new ComboOption { Value = "", Header = "<None> - <无>" });
            config.RepairActions.Add(new ComboOption { Value = "01", Header = "01 - adjusted - 调整" });
            config.RepairActions.Add(new ComboOption { Value = "02", Header = "02 - calibrated - 校准" });
            config.RepairActions.Add(new ComboOption { Value = "03", Header = "03 - checked - 检查" });
            config.RepairActions.Add(new ComboOption { Value = "04", Header = "04 - cleaned - 清洁" });
            config.RepairActions.Add(new ComboOption { Value = "05", Header = "05 - erased - 擦除/清除" });
            config.RepairActions.Add(new ComboOption { Value = "06", Header = "06 - filled - 填充/加料" });
            config.RepairActions.Add(new ComboOption { Value = "07", Header = "07 - fixed - 修复" });
            config.RepairActions.Add(new ComboOption { Value = "08", Header = "08 - lubricated - 润滑" });
            config.RepairActions.Add(new ComboOption { Value = "09", Header = "09 - replaced - 更换" });
            config.RepairActions.Add(new ComboOption { Value = "10", Header = "10 - restored - 恢复" });
            config.RepairActions.Add(new ComboOption { Value = "11", Header = "11 - stretched - 拉伸/收紧" });
            config.RepairActions.Add(new ComboOption { Value = "12", Header = "12 - tightened - 紧固" });
            config.RepairActions.Add(new ComboOption { Value = "13", Header = "13 - trained - 示教/训练" });
            config.RepairActions.Add(new ComboOption { Value = "14", Header = "14 - tuned - 调优" });
            config.RepairActions.Add(new ComboOption { Value = "15", Header = "15 - other - 其他" });

            return config;
        }
    }
}
