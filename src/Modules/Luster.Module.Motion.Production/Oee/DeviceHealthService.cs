using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Production.Oee
{
    /// <summary>
    /// 设备健康/寿命/保养服务（TES-33 P8-E）。
    /// 基于 lmv 既有 <c>IMaintain</c>（MaxHP/UsedHP/MaintainHP/CurrentHP/GetPercent）做 HP 累计 + 保养提醒，
    /// 持久化到 XML（IMaintain 实现的 VirtualDeviceBase 自动序列化）。
    /// </summary>
    /// <remarks>
    /// 源端 <c>LifeTimeEntity</c>/<c>MaintainEntity</c> → lmv <c>IMaintain</c> 接口扩展。
    /// 红黄预警：HP 使用率 ≥ 黄线(MaintainHP) → 黄预警；≥ 红线(MaxHP) → 红预警。
    /// </remarks>
    public static class DeviceHealthService
    {
        /// <summary>
        /// 评估设备健康状态（基于 IMaintain HP 使用率）。
        /// </summary>
        /// <param name="usedPercent">HP 已用百分比（0~100，来自 IMaintain.GetPercent）</param>
        /// <param name="yellowThreshold">黄线阈值（默认 80）</param>
        /// <param name="redThreshold">红线阈值（默认 95）</param>
        public static DeviceHealthLevel EvaluateHealth(double usedPercent, double yellowThreshold = 80, double redThreshold = 95)
        {
            if (usedPercent >= redThreshold) return DeviceHealthLevel.Red;
            if (usedPercent >= yellowThreshold) return DeviceHealthLevel.Yellow;
            return DeviceHealthLevel.Green;
        }

        /// <summary>
        /// 生成保养提醒（对齐源端 MaintainEntity 保养提示 + lmv IMaintain.GetMaintainTips）。
        /// </summary>
        /// <param name="deviceName">设备名</param>
        /// <param name="usedPercent">HP 已用百分比</param>
        /// <param name="remainHP">剩余 HP</param>
        /// <param name="yellowThreshold">黄线阈值</param>
        /// <param name="redThreshold">红线阈值</param>
        public static MaintenanceTip BuildTip(string deviceName, double usedPercent, double remainHP,
            double yellowThreshold = 80, double redThreshold = 95)
        {
            var level = EvaluateHealth(usedPercent, yellowThreshold, redThreshold);
            string message;
            switch (level)
            {
                case DeviceHealthLevel.Red:
                    message = $"[{deviceName}] 寿命已达红线({usedPercent:F1}%)，请立即保养";
                    break;
                case DeviceHealthLevel.Yellow:
                    message = $"[{deviceName}] 寿命达黄线({usedPercent:F1}%)，剩余 HP={remainHP:F1}，请安排保养";
                    break;
                default:
                    message = $"[{deviceName}] 健康({usedPercent:F1}%)";
                    break;
            }
            return new MaintenanceTip(level, deviceName, usedPercent, remainHP, message);
        }

        /// <summary>
        /// 批量评估设备健康（设备名 → 已用百分比），返回需预警的设备列表。
        /// </summary>
        public static IReadOnlyList<MaintenanceTip> EvaluateBatch(
            IReadOnlyDictionary<string, double> deviceUsedPercents,
            IReadOnlyDictionary<string, double> deviceRemainHPs = null,
            double yellowThreshold = 80, double redThreshold = 95)
        {
            var tips = new List<MaintenanceTip>();
            if (deviceUsedPercents == null) return tips;
            foreach (var kv in deviceUsedPercents)
            {
                double remain = deviceRemainHPs != null && deviceRemainHPs.TryGetValue(kv.Key, out var r) ? r : 0;
                var tip = BuildTip(kv.Key, kv.Value, remain, yellowThreshold, redThreshold);
                if (tip.Level != DeviceHealthLevel.Green) tips.Add(tip);
            }
            return tips;
        }
    }

    /// <summary>设备健康等级（红黄绿）</summary>
    public enum DeviceHealthLevel
    {
        /// <summary>健康</summary>
        Green = 0,
        /// <summary>黄线预警（安排保养）</summary>
        Yellow = 1,
        /// <summary>红线预警（立即保养）</summary>
        Red = 2
    }

    /// <summary>保养提醒</summary>
    public sealed class MaintenanceTip
    {
        public DeviceHealthLevel Level { get; }
        public string DeviceName { get; }
        public double UsedPercent { get; }
        public double RemainHP { get; }
        public string Message { get; }

        public MaintenanceTip(DeviceHealthLevel level, string deviceName, double usedPercent, double remainHP, string message)
        {
            Level = level; DeviceName = deviceName; UsedPercent = usedPercent; RemainHP = remainHP; Message = message;
        }
    }
}
