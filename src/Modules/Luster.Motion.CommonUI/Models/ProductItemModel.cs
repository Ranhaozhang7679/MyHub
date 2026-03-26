#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProductItem
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       ProductItem.cs
* 创建时间:       2022/9/16 12:00:58
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      7298d190-502e-4f65-afe3-c5f13bd8af93
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/16 12:00:58
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI
{
    public class ProductItemModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 输出结果
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// 入站时间
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 出站时间
        /// </summary>
        public DateTime OutTime { get; set; }

        /// <summary>
        /// 治具条码
        /// </summary>
        public string JigCode { get; set; } = "";

        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProCode { get; set; } = "";

        /// <summary>
        /// 产品结果
        /// </summary>
        public bool Result { get; set; } = true;

        /// <summary>
        /// NG原因
        /// </summary>
        public string NgReason { get; set; }

        /// <summary>
        /// 是否抛料
        /// </summary>
        public bool IsToss { get; set;} = false;

        /// <summary>
        /// 单片耗时
        /// </summary>
        public double CT { get; set; }


        /// <summary>
        /// CT(2次连续出料事件时间差)
        /// </summary>
        public double NewCT { get; set; }

    }
}
