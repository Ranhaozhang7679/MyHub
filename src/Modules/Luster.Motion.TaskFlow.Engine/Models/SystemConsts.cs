#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SystemConsts
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       SystemConsts.cs
* 创建时间:       2023/2/20 17:12:07
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9ecc4bcc-e542-4724-8972-ae2d8b9d0526
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/20 17:12:07
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class SystemConsts
    {
        public readonly static string ProjectSaved = "项目保存成功";
        public readonly static string ProductMode = "生产模式";
        public readonly static string EmptyMode = "空跑模式";
        public readonly static string FirstMode = "首件模式";

        /// <summary>
        /// 设备换料
        /// </summary>
        public static string PlanDT = "设备换料";

        /// <summary>
        /// 设备换料
        /// </summary>
        public static string Block = "下游堵塞";

        /// <summary>
        /// 午休
        /// </summary>
        public static string NoonBreak = "午休";

        /// <summary>
        /// 白班
        /// </summary>
        public static string Morning = "白班";

        /// <summary>
        /// 夜班
        /// </summary>
        public static string Night = "夜班";
    }
}