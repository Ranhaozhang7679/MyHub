using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.ComponentModel;
using System.IO;

namespace Luster.Module.Motion.FiveAxis.Functions
{
    /// <summary>
    /// 单点激光测距算子节点(P3-A,TES-99)。
    /// 在当前 Z 位置触发单点测距,输出激光读数 + 当前 Z 高度,供 <see cref="LaserZCalibrateNode"/> 两点 Z 标定采点。
    /// </summary>
    /// <remarks>
    /// <b>源端对照</b>(SP-2025140):
    /// - 对应 <c>Station.GetLaserValue(out double)</c>(Check5AxisStationBase.cs:313)+ <c>MActZ.GetCurrentPosition</c>(Form5Cali.cs:388/401 采点)。
    /// - 源端为光谱共焦(<c>$MRO</c> 协议),单值测距。
    ///
    /// <b>运行模式</b>:
    /// - <b>虚拟模式</b>(<c>LineDevice.Virtual.Mode == DeviceMode.Virtual</c>):读 <c>VLineLaser.LineLaserPath</c> 指向的离线点云(.ply/.txt),
    ///   取点云中心 Z 作为激光读数 —— 复用 LaserScan 的 VCloud 离线范式,虚拟模式可跑(不依赖真机)。
    /// - <b>真机模式</b>:经 <see cref="ILaserController"/> 单点读值。真机单点激光设备 ⚠️ 待人类现场接入(TES-57 carve-out),
    ///   未接入时返回失败 + 明确 carve-out 提示,不编造读数。
    ///
    /// <b>R1 非侵入</b>:本节点为 FiveAxis 叶子模块新增算子,平台主干零改动。
    /// </remarks>
    public class LaserMeasureNode : MotionFunction
    {
        /// <summary>激光设备(虚拟:VLineLaser 离线点云;真机:ILaserController 单点设备)</summary>
        [NotEmpty]
        [Parameter("激光设备", 0, CN = "激光设备", EditorType = typeof(VLineLaser))]
        public virtual VDevice LineDevice { get; set; }

        /// <summary>Z 轴(读取当前 Z 高度作为标定采样点)</summary>
        [Parameter("Z轴(采当前高度)", 1, CN = "Z轴")]
        public virtual VAxisDevice ZAxis { get; set; }

        /// <summary>激光读数(厂家原生单位,对应源端 GetLaserValue 返回值)</summary>
        [Parameter("激光读数", 10, CN = "激光读数", ParamType = ParamType.OUT)]
        public virtual double LaserValue { get; set; }

        /// <summary>当前 Z 轴高度(mm,对应源端 MActZ.GetCurrentPosition)</summary>
        [Parameter("当前Z高度", 11, CN = "当前Z", ParamType = ParamType.OUT)]
        public virtual double CurrentZ { get; set; }

        public LaserMeasureNode()
        {
            this.Tips = "单点激光测距";
            this.Icon = "\xe68b";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            LaserValue = InvalidReadings.InvalidDistance;
            CurrentZ = 0;

            // 空跑模式直接返回
            if (IsEmptyMode)
            {
                return true;
            }

            // 当前 Z 高度(源端 MActZ.GetCurrentPosition)
            if (ZAxis != null)
            {
                GetVDevice<VAxis>(ZAxis, out var zAxis);
                if (zAxis != null)
                {
                    CurrentZ = zAxis.GetCurrentPos();
                }
            }

            GetVDevice<VLineLaser>(LineDevice, out var vLaser);
            if (vLaser == null)
            {
                errMsg = "未配置激光设备";
                return false;
            }

            // 虚拟模式:读离线点云,取中心 Z 作为单点激光读数(复用 LaserScan 的 VCloud 离线范式)
            if (DeviceMode.Virtual == LineDevice.Virtual.Mode)
            {
                LaserValue = ReadVirtualDistance(vLaser, out errMsg);
                return !double.IsNaN(LaserValue) && string.IsNullOrEmpty(errMsg);
            }

            // 真机模式:经 ILaserController 单点读值(真机设备待人类现场接入)
            errMsg = "真机单点激光(ILaserController)待人类现场接入:虚拟模式请将激光设备 Mode 设为 Virtual 并配置离线点云路径。";
            MyOwner?.OnLog(Luster.Common.DataStruct.Enums.LogType.Warning, $"LaserMeasure: {errMsg}");
            return false;
        }

        /// <summary>
        /// 虚拟模式:读 <paramref name="vLaser"/> 离线点云,取中心 Z 作为单点激光读数。
        /// 复用 <see cref="Luster.ThreeD.Algorithm.VCloud"/> 离线读取(LaserScan 范式)。
        /// </summary>
        /// <returns>激光读数;失败返回 <see cref="double.NaN"/> 并置 <paramref name="errMsg"/></returns>
        internal static double ReadVirtualDistance(VLineLaser vLaser, out string errMsg)
        {
            errMsg = string.Empty;
            var path = vLaser?.LineLaserPath?.Path;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                errMsg = $"虚拟激光离线点云文件不存在:{path ?? "(未配置)"}";
                return double.NaN;
            }

            var cloud = Luster.ThreeD.Algorithm.VCloud.ReadFile(path, true, out errMsg);
            if (!string.IsNullOrEmpty(errMsg) || cloud == null || cloud.PointNum <= 0)
            {
                errMsg = $"虚拟激光离线点云读取失败:{errMsg ?? "点云为空"}";
                return double.NaN;
            }

            // 取点云中心 Z 作为单点测距读数(标定球/平面采样场景的代表距离)
            var center = cloud.GetCenter();
            return center?.Z ?? double.NaN;
        }
    }
}
