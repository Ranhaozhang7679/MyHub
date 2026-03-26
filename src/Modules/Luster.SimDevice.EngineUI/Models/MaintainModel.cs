#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MaintainModel
* 机器名称:       L05590
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       MaintainModel.cs
* 创建时间:       2023/3/7 16:12:03
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      7b719842-95dc-4a30-b096-95f1d53d88d5
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/3/7 16:12:03
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class MaintainModel : IMaintain
    {
        public string Name { get; set; }

        public double MaxHP { get; set; }

        public double UsedHP { get; set; }

        public double CurrentHP { get; set; }
        public double MaintainHP { get; set; }
        public string Unit { get; set; }
        public Guid ID { get; set; }

        public virtual double GetPercent()
        {
            if (MaxHP == 0) return 0;

            return Math.Round(CurrentHP / MaintainHP * 100, 2);
        }

        public virtual string GetActual()
        {
            return $"{CurrentHP} / {MaintainHP} {Unit}";
        }

        public string GetMaintainTips()
        {
            return string.Empty;
        }
    }
}