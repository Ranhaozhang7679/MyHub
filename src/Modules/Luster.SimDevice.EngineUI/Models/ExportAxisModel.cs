#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ExportAxisModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       ExportAxisModel.cs
* 创建时间:       2022/10/27 10:44:34
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      f2849d97-f00d-46db-a9ae-e7f63c579314
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/27 10:44:34
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class ExportAxisModel
    {
        [DisplayName("序号")]
        public int Index { get; set; }

        [DisplayName("地址")]
        public string AxisNo { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("模块")]
        public string ModuleName { get; set; }

        [DisplayName("加速度(单位mm/s^2)")]
        public int AccSpeed { get; set; }

        [DisplayName("减速度(单位mm/s^2)")]
        public int DecSpeed { get; set; }

        [DisplayName("运动速度(mm/s)")]
        public double MoveSpeed { get; set; }

        [DisplayName("脉冲毫米比(pulse/mm)")]
        public int Pluse { get; set; }

        [DisplayName("回零高速(mm/s)")]
        public int HomeSpeedHigh { get; set; }

        [DisplayName("回零低速(mm/s)")]
        public int HomeSpeedLow { get; set; }

        /// <summary>
        /// 回零偏移
        /// </summary>
        [DisplayName("回零偏移")]
        public float HomeOffset { get; set; }

        [DisplayName("回零方式")]
        public int HomeMode { get; set; }

        [DisplayName("回零优先级")]
        public int Priority { get; set; }

        [DisplayName("Jog速度(mm/s)")]
        public double JogSpeed { get; set; }

        /// <summary>
        /// 正向最大行程
        /// </summary>
        [DisplayName("正向最大行程")]
        public float SoftLimitPL { get; set; }

        /// <summary>
        /// 负向最大行程
        /// </summary>
        [DisplayName("负向最大行程")]
        public float SoftLimitML { get; set; }

        /// <summary>
        /// 到位稳定时间
        /// </summary>
        [DisplayName("到位稳定时间")]
        public int Stime { get; set; }

        /// <summary>
        /// 到位判断脉冲数
        /// </summary>
        [DisplayName("到位判断脉冲数")]
        public int AxisBand { get; set; }

        /// <summary>
        /// 回零加速度
        /// </summary>
        [DisplayName("回零加速度")]
        public uint HomeAcc { get; set; }

        /// <summary>
        /// 回零减速度
        /// </summary>
        [DisplayName("回零减速度")]
        public uint HomeDec { get; set; }

        /// <summary>
        /// 轴类型
        /// </summary>
        [DisplayName("轴类型")]
        public AxisType AxisType { get; set; }


        /// <summary>
        /// 是否使用齿轮箱
        /// </summary>
        [DisplayName("是否使用齿轮箱")]
        public bool HaveTransmission { get; set; }


        /// <summary>
        /// 指令脉冲
        /// </summary>
        [DisplayName("指令脉冲")]
        public int CommandPulse { get; set; }

        /// <summary>
        /// 一圈移动量
        /// </summary>
        [DisplayName("一圈移动量")]
        public int CircleDisplacement { get; set; }


        /// <summary>
        /// 齿轮比分子
        /// </summary>
        [DisplayName("齿轮比分子")]
        public int GearRatioNumerator { get; set; }

        /// <summary>
        /// 齿轮比分母
        /// </summary>
        [DisplayName("齿轮比分母")]
        public int GearRatioDenominator { get; set; }


    }
}
