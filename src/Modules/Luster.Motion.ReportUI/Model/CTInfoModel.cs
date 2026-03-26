#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CTInfoModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.Model
* 文 件 名:       CTInfoModel.cs
* 创建时间:       2022/8/24 16:07:52
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      a2a880a8-8d1c-4b21-b13b-f0b3afab6df7
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/24 16:07:52
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.ReportUI.Model
{
    public class CTInfoModel
    {
        /// <summary>
        /// id号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 动作名称
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 耗时
        /// </summary>
        public double CtTime { get; set; }

        /// <summary>
        /// 标准耗时
        /// </summary>
        public double StandardCtTime { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string SnCode { get; set; }

        /// <summary>
        /// CT差异
        /// </summary>
        public double Differ { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
        public DateTime CreateTime { get; set; }

        public float XSpd { get; set; }
        public float XSpdTarget { get; set; }
        public float XAcc { get; set; }
        public float XAccTarget { get; set; }

        public float YSpd { get; set; }
        public float YSpdTarget { get; set; }
        public float YAcc { get; set; }
        public float YAccTarget { get; set; }

        public float ZSpd { get; set; }
        public float ZSpdTarget { get; set; }
        public float ZAcc { get; set; }
        public float ZAccTarget { get; set; }

        public float USpd { get; set; }
        public float USpdTarget { get; set; }
        public float UAcc { get; set; }
        public float UAccTarget { get; set; }

        public float RSpd { get; set; }
        public float RSpdTarget { get; set; }
        public float RAcc { get; set; }
        public float RAccTarget { get; set; }

        public int Delay { get; set; }


        //public float XDec { get; set; }
        //public float XDistance { get; set; }

        //public float YSpd { get; set; }
        //public float YAcc { get; set; }
        //public float YDec { get; set; }
        //public float YDistance { get; set; }


        //public float ZSpd { get; set; }
        //public float ZAcc { get; set; }
        //public float ZDec { get; set; }
        //public float ZDistance { get; set; }



        //public float USpd { get; set; }
        //public float UAcc { get; set; }
        //public float UDec { get; set; }
        //public float UDistance { get; set; }
    }
}
