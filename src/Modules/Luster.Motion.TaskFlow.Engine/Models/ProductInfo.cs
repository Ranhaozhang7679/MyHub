#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProEnterEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       ProEnterEvent.cs
* 创建时间:       2022/7/27 8:51:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6cb47bc1-5fa6-4016-954c-dec740efe79d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/27 8:51:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 产品信息
    /// </summary>
    public class ProductInfo
    {
        /// <summary>
        /// 最终结果
        /// </summary>
        public StationResult Result { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 输出结果
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 入站时间
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 出站时间
        /// </summary>
        public DateTime OutTime { get; set; }

        /// <summary>
        /// 获取CT
        /// </summary>
        /// <returns></returns>
        public double GetCT()
        {
            return Math.Round((OutTime - EnterTime).TotalSeconds, 2);
        }
    }
}