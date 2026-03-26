#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbCTInfo
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Tables
* 文 件 名:       TbCTInfo.cs
* 创建时间:       2022/10/14 10:12:07
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      ddeeb8e1-14e8-41fb-bbc3-7ec787538b09
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:12:07
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql.DataAnnotations;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    [Table(Name = "CTInfoTable")]
    public class TbCTInfo : BaseTable
    {
        [PropSort(1), DisplayName("SN编码")]
        public string SNCode { get; set; }

        [PropSort(2), DisplayName("模块")]
        public string StationName { get; set; }

        [PropSort(3), DisplayName("动作")]
        public string ModuleName { get; set; }

        [PropSort(4), DisplayName("开始时间")]
        public DateTime StartTime { get; set; }

        [PropSort(5), DisplayName("结束时间")]
        public DateTime EndTime { get; set; }

        [PropSort(6), DisplayName("运行CT")]
        public double CtTime { get; set; }

        [PropSort(7), DisplayName("标准CT")]
        public double StandardCTTime { get; set; }

        [PropSort(8), DisplayName("差值")]
        public double Differ { get; set; }

        [PropSort(9), DisplayName("X轴速度")]
        public float XSpd { get; set; }

        [PropSort(10), DisplayName("X轴加速度")]
        public float XAcc { get; set; }

        [PropSort(11), DisplayName("X轴目标速度")]
        public float XSpdTarget { get; set; }

        [PropSort(12), DisplayName("X轴目标加速度")]
        public float XAccTarget { get; set; }

        [PropSort(13), DisplayName("Y轴速度")]
        public float YSpd { get; set; }

        [PropSort(14), DisplayName("Y轴加速度")]
        public float YAcc { get; set; }

        [PropSort(15), DisplayName("Y轴目标速度")]
        public float YSpdTarget { get; set; }

        [PropSort(16), DisplayName("Y轴目标加速度")]
        public float YAccTarget { get; set; }

        [PropSort(17), DisplayName("Z轴速度")]
        public float ZSpd { get; set; }

        [PropSort(18), DisplayName("Z轴加速度")]
        public float ZAcc { get; set; }

        [PropSort(19), DisplayName("Z轴目标速度")]
        public float ZSpdTarget { get; set; }

        [PropSort(20), DisplayName("Z轴目标加速度")]
        public float ZAccTarget { get; set; }

        [PropSort(21), DisplayName("U轴速度")]
        public float USpd { get; set; }

        [PropSort(22), DisplayName("U轴加速度")]
        public float UAcc { get; set; }

        [PropSort(23), DisplayName("U轴目标速度")]
        public float USpdTarget { get; set; }

        [PropSort(24), DisplayName("U轴目标加速度")]
        public float UAccTarget { get; set; }

        [PropSort(25), DisplayName("R轴速度")]
        public float RSpd { get; set; }

        [PropSort(26), DisplayName("R轴加速度")]
        public float RAcc { get; set; }

        [PropSort(27), DisplayName("R轴目标速度")]
        public float RSpdTarget { get; set; }

        [PropSort(28), DisplayName("R轴目标加速度")]
        public float RAccTarget { get; set; }

        [PropSort(29), DisplayName("延时")]
        public int Delay { get; set; }


        //[PropSort(8), DisplayName("Y轴速度")]
        //public float YSpd { get; set; }
        //[PropSort(9), DisplayName("Z轴速度")]
        //public float ZSpd { get; set; }
        //[PropSort(10), DisplayName("U轴速度")]
        //public float USpd { get; set; }


        //[PropSort(11), DisplayName("X轴运动距离")]
        //public float XDistance { get; set; }
        //[PropSort(12), DisplayName("Y轴运动距离")]
        //public float YDistance { get; set; }
        //[PropSort(13), DisplayName("Z轴运动距离")]
        //public float ZDistance { get; set; }
        //[PropSort(14), DisplayName("U轴运动距离")]
        //public float UDistance { get; set; }



        //[PropSort(16), DisplayName("Y轴加速时间")]
        //public float YAcc { get; set; }
        //[PropSort(17), DisplayName("Z轴加速时间")]
        //public float ZAcc { get; set; }
        //[PropSort(18), DisplayName("U轴加速时间")]
        //public float UAcc { get; set; }

        //[PropSort(19), DisplayName("X轴减速时间")]
        //public float XDec { get; set; }

        //[PropSort(20), DisplayName("Y轴减速时间")]
        //public float YDec { get; set; }
        //[PropSort(21), DisplayName("Z轴减速时间")]
        //public float ZDec { get; set; }
        //[PropSort(22), DisplayName("U轴减速时间")]
        //public float UDec { get; set; }



        ///// <summary>
        ///// 扩展参数
        ///// </summary>
        //[PropSort(7)]
        //public Dictionary<string, object> ExtParams { get; set; }


    }

    // 2025-5-8 新增一个csv表格
    [Table(Name = "CTInfoTable2")]
    public class TbCTInfo2 : BaseTable
    {
        [PropSort(1), DisplayName("SN")]
        public string SN { get; set; }

        [PropSort(2), DisplayName("模块")]
        public string 模块 { get; set; }

        [PropSort(3), DisplayName("动作")]
        public string 动作 { get; set; }

        [PropSort(4), DisplayName("开始时间")]
        public DateTime 开始时间 { get; set; }

        [PropSort(5), DisplayName("结束时间")]
        public DateTime 结束时间 { get; set; }

        [PropSort(6), DisplayName("Actual_CT")]  // 运行CT
        public double Actual_CT { get; set; }

        [PropSort(7), DisplayName("Target_CT")]  // 标准CT
        public double Target_CT { get; set; }

        [PropSort(8), DisplayName("Gap")]        // 差值
        public double Gap { get; set; }

        [PropSort(9), DisplayName("Time_Slot")]  // 每个周期/每片料的开始时间
        public DateTime Time_Slot { get; set; }
    }

}
