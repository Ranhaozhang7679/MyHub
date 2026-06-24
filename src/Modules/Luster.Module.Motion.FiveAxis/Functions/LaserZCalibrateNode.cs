using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Service;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.FiveAxis.Functions
{
    /// <summary>
    /// 激光 Z 单点标定算子节点(P3-A,TES-99)。
    /// 由两点激光读数+Z 高度 + 标准值 + 激光/相机示教位置,经 <see cref="IFiveAxisCalibrationService.LaserCalibrate"/>
    /// 计算 LaserMap(LinearConverter 激光读数↔Z 高度)。对应源端 <c>Form5Cali.laserCaliApply</c>(Form5Cali.cs:281-295)。
    /// </summary>
    /// <remarks>
    /// <b>源端流程</b>:示教两点 → 各点采 (激光读数, Z 高度) → 填 LinearConverter(Map1/Map2:DirectValue=激光读数,UnitValue=Z 高度)
    /// → 记录 LaserPosi/CameraPosi(派生 CameraOffset = CameraPosi - LaserPosi)。本节点把该流程抽成可 recipe 编排的算子。
    ///
    /// 两点采样由上游 <see cref="LaserMeasureNode"/> 产出(LaserValue/CurrentZ),经 recipe 参数引用接入 Laser1/Z1、Laser2/Z2;
    /// 亦支持手动输入(标定调试/软件验收)。标定结果落 <see cref="CalibratedResult"/>,供下游检测站 Z 修正使用。
    ///
    /// <b>R1 非侵入</b>:FiveAxis 叶子模块新增算子,平台主干零改动;算法本体复用 P5-5b Service(纯 C#,软件可验)。
    /// </remarks>
    public class LaserZCalibrateNode : MotionFunction
    {
        /// <summary>采样点1:激光读数</summary>
        [Parameter("采样点1激光读数", 0, Group = "采样点1", CN = "激光读数1", DefaultV = 0.0)]
        public virtual double Laser1 { get; set; }

        /// <summary>采样点1:Z 轴高度(mm)</summary>
        [Parameter("采样点1Z高度", 1, Group = "采样点1", CN = "Z高度1", DefaultV = 0.0)]
        public virtual double Z1 { get; set; }

        /// <summary>采样点2:激光读数</summary>
        [Parameter("采样点2激光读数", 2, Group = "采样点2", CN = "激光读数2", DefaultV = 1.0)]
        public virtual double Laser2 { get; set; }

        /// <summary>采样点2:Z 轴高度(mm)</summary>
        [Parameter("采样点2Z高度", 3, Group = "采样点2", CN = "Z高度2", DefaultV = 1.0)]
        public virtual double Z2 { get; set; }

        /// <summary>标准测量值(源端 LaserStandard,标定基准)</summary>
        [Parameter("标准测量值", 4, Group = "标定基准", CN = "标准值", DefaultV = 0.0)]
        public virtual double LaserStandard { get; set; }

        /// <summary>激光示教位置 X</summary>
        [Parameter("激光示教位置X", 10, Group = "示教位置", CN = "激光PosiX", DefaultV = 0.0)]
        public virtual double LaserPosiX { get; set; }

        /// <summary>激光示教位置 Y</summary>
        [Parameter("激光示教位置Y", 11, Group = "示教位置", CN = "激光PosiY", DefaultV = 0.0)]
        public virtual double LaserPosiY { get; set; }

        /// <summary>激光示教位置 Z</summary>
        [Parameter("激光示教位置Z", 12, Group = "示教位置", CN = "激光PosiZ", DefaultV = 0.0)]
        public virtual double LaserPosiZ { get; set; }

        /// <summary>相机示教位置 X</summary>
        [Parameter("相机示教位置X", 13, Group = "示教位置", CN = "相机PosiX", DefaultV = 0.0)]
        public virtual double CameraPosiX { get; set; }

        /// <summary>相机示教位置 Y</summary>
        [Parameter("相机示教位置Y", 14, Group = "示教位置", CN = "相机PosiY", DefaultV = 0.0)]
        public virtual double CameraPosiY { get; set; }

        /// <summary>相机示教位置 Z</summary>
        [Parameter("相机示教位置Z", 15, Group = "示教位置", CN = "相机PosiZ", DefaultV = 0.0)]
        public virtual double CameraPosiZ { get; set; }

        /// <summary>标定斜率 k(Z= k*激光读数 + b),OUT 便于 ParamGrid 查看</summary>
        [Parameter("标定斜率k", 50, Group = "输出", CN = "斜率k", ParamType = ParamType.OUT)]
        public virtual double CalibratedK { get; set; }

        /// <summary>标定截距 b</summary>
        [Parameter("标定截距b", 51, Group = "输出", CN = "截距b", ParamType = ParamType.OUT)]
        public virtual double CalibratedB { get; set; }

        /// <summary>标定结果(供下游代码访问;LinearConverter 激光读数↔Z 高度)</summary>
        public LaserCaliResult CalibratedResult { get; private set; }

        public LaserZCalibrateNode()
        {
            this.Tips = "激光Z单点标定";
            this.Icon = "\xe6e1";
            CalibratedResult = new LaserCaliResult();
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            // 采样点重合无法定标(LinearConverter 分母 DirectValue 差为 0)
            if (Math.Abs(Laser2 - Laser1) < 1e-12)
            {
                errMsg = "两点激光读数相同,无法进行 Z 标定(需两个不同 Z 高度的采样点)";
                return false;
            }

            // 调 P5-5b 标定 Service:两点激光读数+Z 高度 → LinearConverter + 示教位置
            var service = new FiveAxisCalibrationService();
            var laserPosi = new PositionXYZ(LaserPosiX, LaserPosiY, LaserPosiZ);
            var cameraPosi = new PositionXYZ(CameraPosiX, CameraPosiY, CameraPosiZ);

            bool ok = service.LaserCalibrate(CalibratedResult, Laser1, Z1, Laser2, Z2, LaserStandard, laserPosi, cameraPosi);
            if (!ok)
            {
                errMsg = "激光 Z 标定 Service 执行失败";
                return false;
            }

            // 暴露 k/b(与 LinearConverter.GetConvertFactor 同式:k=(z2-z1)/(laser2-laser1), b=z1-k*laser1)
            CalibratedK = (Z2 - Z1) / (Laser2 - Laser1);
            CalibratedB = Z1 - CalibratedK * Laser1;

            MyOwner?.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug,
                $"LaserZCalibrate: ({Laser1},{Z1})/({Laser2},{Z2}) -> k={CalibratedK}, b={CalibratedB}, standard={LaserStandard}");

            return true;
        }
    }
}
