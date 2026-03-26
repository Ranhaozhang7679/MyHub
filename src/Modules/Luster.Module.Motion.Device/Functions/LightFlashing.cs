#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GetIO
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       GetIO.cs
* 创建时间:       2022/5/30 9:35:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dd4298e6-900c-45dc-81e4-8758470f2a3f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 9:35:33
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 获取IO值
    /// </summary>
    public class LightFlashing : MotionFunction
    {
        [Parameter("对哪个灯进行开启", 1, CN = "开灯类型", DefaultV = LightType.Red)]
        public LightType LightType { get; set; }

        [Parameter("是否开启开启蜂鸣器,如果为否，则关闭蜂鸣器", 2, CN = "开启蜂鸣器", DefaultV = false)]
        public bool IsBuzzer { get; set; }

        [DependOn("IsBuzzer", true)]
        [Parameter("中断灯闪烁的Io数量", 4, CN = "蜂鸣器间隔", DefaultV = 200)]
        public int Interval { get; set; }

        public LightFlashing()
        {
            this.Tips = "灯闪烁";
            this.Icon = "\xe67f";
        }

        public override bool DoExcute(out string errMsg)
        {
            MyOwner.LightManager.OpenLight(LightType, IsBuzzer, Interval);
            return base.DoExcute(out errMsg);
        }
    }
}