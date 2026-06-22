using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System.Collections.Generic;
using System.Linq;

namespace Luster.Module.Motion.HomeProfile.Functions
{
    /// <summary>
    /// 板卡初始化链路校验节点（TES-39 P7-D）。
    /// 对齐源端 <c>BoardInitTaskBase</c>（<c>BoardInitTaskBase.cs:29-111</c>）板卡初始化链路：
    /// 源端 <c>doProc</c> → <c>BoardInitial</c> → <c>stationInitial</c> → <c>addTask</c>
    /// （IORefresh/ComponentRegister/DisplayRefresh）→ <c>endInitial</c>（LoadProfileValue）。
    /// lmv 对应 <c>DeviceEngine.SetEngineMode</c>（<c>DeviceEngine.cs:1103</c>）已做 InitApi/Open/SetEnabled，
    /// 本节点做初始化后校验：确认设备引擎已初始化 + 各轴可通信（GetAxisStatus 不抛）+ 关键设备在位。
    /// </summary>
    /// <remarks>
    /// 源端 <c>ComponentRegisterTask</c>（二进制，<c>CommonMachineModelLibrary.Task</c>）在 lmv 无对应——
    /// lmv 设备注册走 <c>DeviceEngine.Initialize</c> 反射（<c>DeviceEngine.cs:264</c>），不需照搬。
    /// 本节点只做初始化结果校验，不重建注册流程（范围冻结）。
    /// 真机板卡初始化 ⚠️ 待人类现场验证。
    /// </remarks>
    public class AxisInitVerifier : MotionFunction
    {
        /// <summary>待校验轴列表（确认各轴可通信）</summary>
        [Parameter("待校验轴列表", 0, CN = "校验轴列表")]
        public List<VDevice> Axes { get; set; } = new List<VDevice>();

        /// <summary>是否要求所有轴已回零（true=校验 IsHome）</summary>
        [Parameter("是否要求已回零", 1, CN = "要求已回零", DefaultV = false)]
        public bool RequireHomed { get; set; } = false;

        /// <summary>未通过校验的轴名（OUT）</summary>
        [Parameter("未通过校验轴", 10, CN = "未通过轴", ParamType = ParamType.OUT)]
        public string FailedAxes { get; set; }

        public AxisInitVerifier()
        {
            this.Tips = "板卡初始化链路校验(对齐源端BoardInitTask)";
            this.Icon = "\xe6a3";
        }

        public override string[] NoteParams { get; set; } = new[] { nameof(RequireHomed) };

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            FailedAxes = string.Empty;

            // 1. 校验设备引擎已初始化（对齐源端 BoardInitial 完成）
            if (MyOwner?.DeviceEngine == null)
            {
                errMsg = "设备引擎未初始化（DeviceEngine 为 null）";
                OnAlarm(AlarmType.DeviceError, errMsg, "INIT_NO_ENGINE");
                return false;
            }

            // 2. 校验各轴可通信（GetAxisStatus 不抛 = 卡端通信正常，对齐源端 IORefresh 设备在线检查）
            var failed = new List<string>();
            if (Axes != null)
            {
                foreach (var device in Axes)
                {
                    GetVDevice<VAxis>(device, out var axis);
                    if (axis == null)
                    {
                        failed.Add(device?.Name ?? "?");
                        continue;
                    }

                    try
                    {
                        // 读取轴状态确认通信（不抛异常视为可通信）
                        var status = axis.GetAxisStatus(false);
                        if (status == null)
                        {
                            failed.Add(axis.Name);
                            continue;
                        }

                        // 可选：校验已回零
                        if (RequireHomed && !axis.IsHome)
                        {
                            failed.Add(axis.Name + "(未回零)");
                        }
                    }
                    catch
                    {
                        failed.Add(axis.Name + "(通信异常)");
                    }
                }
            }

            if (failed.Count > 0)
            {
                FailedAxes = string.Join(",", failed);
                errMsg = $"板卡初始化校验失败，未通过轴：{FailedAxes}";
                OnAlarm(AlarmType.DeviceError, errMsg, "INIT_VERIFY_FAIL");
                return false;
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 初始化校验判定（纯逻辑，便于单测）。
        /// 对齐源端 <c>BoardInitTaskBase.endInitial</c> 后设备在线校验。
        /// </summary>
        /// <param name="engineReady">设备引擎是否已初始化</param>
        /// <param name="axisStatusOk">各轴状态读取是否正常（可通信）</param>
        /// <param name="allHomed">若要求已回零，是否全部已回零</param>
        /// <param name="requireHomed">是否要求已回零</param>
        /// <returns>true=校验通过</returns>
        public static bool EvaluateInit(bool engineReady, bool axisStatusOk, bool allHomed, bool requireHomed)
        {
            if (!engineReady) return false;
            if (!axisStatusOk) return false;
            if (requireHomed && !allHomed) return false;
            return true;
        }
    }
}
