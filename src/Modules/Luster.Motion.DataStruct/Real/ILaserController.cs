#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ILaserController
* 命名空间:       Luster.Motion.DataStruct.Real
* 文 件 名:       ILaserController.cs
* 创建时间:       2026/06/24
* 作    者:       全栈工程师
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 创建年份:       2026
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 单点激光测距控制器接口(P3-A,TES-99)。
    /// 光谱共焦等单点测距设备契约:触发单次测距返回一个距离读数,
    /// 区别于线扫 <see cref="ILineLaser"/>(轮廓点云,经 SoftTrigger + ScanFinishEvent 回调返回 LineLaser 矩阵)。
    /// </summary>
    /// <remarks>
    /// <b>源端对照</b>(核实于 SP-2025140):
    /// - 调用模型:源端 <c>Station.GetLaserValue(out double)</c>(Check5AxisStationBase.cs:313)
    ///   → <c>GetSpectralConfocalValue</c>(:268)发 <c>$MRO\r\n</c> 命令,解析响应取单点距离(光谱共焦)。
    /// - 源端只用一种品牌:<c>LaserMode.Model_SpectralConfocal</c>(奥斯泰/松下共焦)。
    ///
    /// <b>目标端落差</b>:lmv 既有 <see cref="ILineLaser"/> 是线扫轮廓设备(点云矩阵),无"读一个数"的单点出口;
    /// <c>LaserSensor</c> 算子走 VIO 模拟量测高,不碰激光设备。故五轴 Z 标定所需"单点激光读数"需新建本接口。
    ///
    /// <b>真机接入</b>:厂家设备实现本接口后,经 <c>IDeviceEngine</c> 发现,供 LaserMeasure 算子单点采值。
    /// 真机激光出数 ⚠️ 待人类现场接入(TES-57 carve-out);虚拟模式经 VLineLaser/VCloud 离线点云读值,不依赖本接口。
    /// </remarks>
    public interface ILaserController
    {
        /// <summary>是否已连接/通讯就绪</summary>
        bool IsConnected { get; }

        /// <summary>
        /// 触发单次测距,返回激光距离读数(厂家原生单位)。
        /// 读数经 <c>LaserCaliResult.LaserMap</c>(LinearConverter)换算为 Z 轴高度。
        /// 对应源端 <c>Station.GetLaserValue(out double)</c>。
        /// </summary>
        /// <returns>距离读数;无效/异常返回 <see cref="InvalidReadings.InvalidDistance"/></returns>
        double GetDistance();
    }

    /// <summary>激光/光源读数哨兵常量(供 <see cref="ILaserController"/> 等设备实现共用)。</summary>
    public static class InvalidReadings
    {
        /// <summary>无效距离读数哨兵值(对应源端 GetLaserValue 异常返回 -999.999)</summary>
        public const double InvalidDistance = -999.999;
    }
}
