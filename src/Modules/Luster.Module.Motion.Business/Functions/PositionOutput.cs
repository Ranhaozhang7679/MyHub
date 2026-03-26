#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PositionOutput
* 机器名称:       Z05150
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       PositionOutput.cs
* 创建时间:       2022/7/21 10:36:18
* 作    者:       张宇
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yuzhang5150@lusterinc.com 
* 唯一标识：      dcf826b0-a981-41ab-8247-d7283c2e09af
* 登录用户:       zhangyu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/21 10:36:18
* 修 改 人:		  Z05150
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 取料方式
    /// </summary>
    public enum TakeCarrierMethod
    {
        [Description("行取料")]
        Row,

        [Description("列取料")]
        Column
    }

    /// <summary>
    /// 位置输出
    /// </summary>
    public class PositionOutput : MotionFunction
    {
        [NotEmpty]
        [Parameter("取料方式", 0, CN = "取料方式", DefaultV = TakeCarrierMethod.Row)]
        public TakeCarrierMethod TakeCarrierMethod { get; set; }

        [NotEmpty]
        [Parameter("多轴仿真设备", 1, CN = "多轴")]
        public VAxisMDevice AxisM { get; set; }

        [NotEmpty]
        [Parameter("行数", 2, CN = "行数", DefaultV = 0)]
        public int RowCount { get; set; }

        [NotEmpty]
        [Parameter("列数", 3, CN = "列数", DefaultV = 0)]
        public int ColumnCount { get; set; }

        [NotEmpty]
        [Parameter("料盘宽度", 4, CN = "料盘宽度", DefaultV = 0)]
        public double TrayWidth { get; set; }

        [NotEmpty]
        [Parameter("料盘高度", 5, CN = "料盘高度", DefaultV = 0)]
        public double TrayHeight { get; set; }

        [Parameter("料盘空料信号", 6, CN = "料盘空料信号", CanRef = ParamRef.Ref)]
        public bool EmptyTray { get; set; }

        #region 变量输出
        [Parameter("X轴位置", 7, CN = "X轴位置", ParamType = ParamType.OUT, CanRef = ParamRef.Ref)]
        public double XPos { get; set; }

        [Parameter("Y轴位置", 8, CN = "Y轴位置", ParamType = ParamType.OUT)]
        public double YPos { get; set; }
        #endregion

        /// <summary>
        /// X方向当前位置、Y方向当前位置
        /// </summary>
        private double XCurrentPos, YCurrentPos = 0;

        /// <summary>
        /// 移动次数、X方向移动次数、Y方向移动次数
        /// </summary>
        private int MoveCount, Xi, Yi = 1;

        public PositionOutput()
        {
            this.Tips = "位置输出";
            this.Icon = "\xe66c";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VAxisM>(AxisM, out var axisM);

            if (MoveCount <= (RowCount * ColumnCount))
            {
                EmptyTray = false;

                // 获取示教位
                for (int i = 1; i <= axisM.Axises.Count; i++)
                {
                    XCurrentPos = Convert.ToDouble(axisM.Axises[0].Position.ToString());
                    YCurrentPos = Convert.ToDouble(axisM.Axises[1].Position.ToString());
                }

                // 计算位置
                XPos = XCurrentPos + Xi * TrayWidth;
                YPos = YCurrentPos + Yi * TrayHeight;

                // 以行的方式取料
                if (TakeCarrierMethod == TakeCarrierMethod.Row)
                {
                    if (Xi <= ColumnCount)
                    {
                        Yi++;
                        if (Yi > RowCount)
                        {
                            Yi = 1;
                            Xi++;
                        }
                    }
                }
                // 以列的方式取料
                else if (TakeCarrierMethod == TakeCarrierMethod.Column)
                {
                    if (Yi <= RowCount)
                    {
                        Xi++;
                        if (Xi > ColumnCount)
                        {
                            Xi = 1;
                            Yi++;
                        }
                    }
                }

                MoveCount++;
            }
            else
            {
                MoveCount = Xi = Yi = 1;

                // 料盘无料
                SetParameterRefVal(nameof(EmptyTray), true);
            }

            return base.DoExcute(out errMsg);
        }
    }
}